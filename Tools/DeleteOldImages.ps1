param(
[string]$spUsername,
[string]$spPassword,
[string]$azureTenant,
[string]$registryName,
[string]$modelsFileShare,
[string[]]$images
)

function Get-TimeStamp {
    return "[{0:MM/dd/yy} {0:HH:mm:ss}]" -f (Get-Date)
}

az login --service-principal -u $spUsername -p $spPassword --tenant $azureTenant

Get-PSDrive | ForEach {
    If ( $_.Name -eq 'GETModels' ) {
        Remove-PSDrive -Name $_.Name -Force
    }
}

$connectTestResult = Test-NetConnection -ComputerName getqa.file.core.windows.net -Port 445
if ($connectTestResult.TcpTestSucceeded) {
    # Save the password so the drive will persist on reboot
    cmd.exe /C "cmdkey /add:`"getqa.file.core.windows.net`" /user:`"Azure\getqa`" /pass:`"passwordhere`""
    # Mount the drive
    New-PSDrive -Name GETModels -PSProvider FileSystem -Root "\\getqa.file.core.windows.net\$modelsFileShare"
} else {
    Write-Error -Message "$(Get-TimeStamp) Unable to reach the Azure storage account via port 445. Check to make sure your organization or ISP is not blocking port 445, or use Azure P2S VPN, Azure S2S VPN, or Express Route to tunnel SMB traffic over a different port."
}

$dirs = Get-ChildItem -Path GETModels:\Containers\ -Filter dockerfile -Recurse -ErrorAction SilentlyContinue -Force
if ($images.Length -gt 0) {
    $dirs = @($dirs | Where-Object {$images -contains $_.Directory.Name})
}
$curr = 0
$total = $dirs.Length

$dirs | ForEach-Object {
    Write-Host "$(Get-TimeStamp) Start $($_.Directory.Name) ($curr of $total)"

	$dirPath = join-path -path "c:\DockerImages" -childpath $_.Directory.Name

    if (-not (Test-Path $dirPath)) {
        New-Item -ItemType Directory -Force -Path $dirPath
    }

    Push-Location $dirPath
    $curr = $curr + 1
    	
    # delete old images from ACR
    az acr repository show-manifests --name $registryName --repository $($_.Directory.Name) --query "[?tags[0]==null].digest" -o tsv | %{ az acr repository delete --name $registryName --image "$($_.Directory.Name)@$_" --yes }
    
    Pop-Location

    Remove-Item $dirPath -Force -Recurse

    Write-Host "$(Get-TimeStamp) End $($_.Directory.Name)"
}

az logout

docker images --filter "dangling=true" -q --no-trunc | Select-Object -Unique | % { docker rmi $_ -f }

Remove-PSDrive -Name GETModels -Force