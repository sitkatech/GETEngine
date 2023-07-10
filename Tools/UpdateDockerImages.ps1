param(
[string]$registryName,
[string]$registryUrl,
[string]$registryUsername,
[string]$registryPassword,
[string]$modelsFileShare,
[string[]]$images
)
Add-Type -AssemblyName System.Web

function Get-TimeStamp {
    return "[{0:MM/dd/yy} {0:HH:mm:ss}]" -f (Get-Date)
}

Get-PSDrive | ForEach {
    If ( $_.Name -eq 'GETModels' ) {
        Remove-PSDrive -Name $_.Name -Force
    }
}

$connectTestResult = Test-NetConnection -ComputerName getqa.file.core.windows.net -Port 445
if ($connectTestResult.TcpTestSucceeded) {
    # Save the password so the drive will persist on reboot
    cmd.exe /C "cmdkey /add:`"getqa.file.core.windows.net`" /user:`"Azure\getqa`" /pass:`"${env:STORAGEACCOUNTPASSWORD}`""
    # Mount the drive
    New-PSDrive -Name GETModels -PSProvider FileSystem -Root "\\getqa.file.core.windows.net\$modelsFileShare"
} else {
    Write-Error -Message "$(Get-TimeStamp) Unable to reach the Azure storage account via port 445. Check to make sure your organization or ISP is not blocking port 445, or use Azure P2S VPN, Azure S2S VPN, or Express Route to tunnel SMB traffic over a different port."
}

docker logout $registryUrl

#this is stupid but doesn't work normal ways when running through Azure DevOps
New-Item "temp.txt" -Value $registryPassword
CMD /c "docker login $registryUrl -u $registryUsername --password-stdin < temp.txt"
Remove-Item "temp.txt"
#end stupid (well stuff after could be stupid as well but should be less stupid)

$dirs = Get-ChildItem -Path GETModels:\Containers\ -Filter dockerfile -Recurse -ErrorAction SilentlyContinue -Force
if ($images.Length -gt 0) {
    $dirs = @($dirs | Where-Object {$images -contains $_.Directory.Name})
}

$curr = 0
$total = $dirs.Length

#Build a base image that has our agent and entry instructions
Push-Location C:\DockerImages
docker build -f BaseImageDockerfile -t $registryName .

$baseImageName = $registryName + ':latest'

Write-Host "$(Get-TimeStamp) Preparing to process $total docker images"
$dirs | ForEach-Object {
    docker system prune -f

    $dir = $_.Directory
    Push-Location $dir
    $curr = $curr + 1
    Write-Host "$(Get-TimeStamp) Start $($dir.Name) ($curr of $total)"
    
    #Update the Model's dockerfile with our base image name
    ((Get-Content -path Dockerfile -Raw) -replace 'REPLACEWITHBASEIMAGENAME', $baseImageName) | Set-Content -Path Dockerfile
    
    #create the image locally
    docker build -t $registryUrl/$($dir.Name) .
    #send it to the azure container registry
    docker push  $registryUrl/$($dir.Name)
    #remove it locally because we no longer need it
    docker rmi $registryUrl/$($dir.Name)
    
    #Switch the dockerfile first line back. This is more necessary for the staging and prod builds
    ((Get-Content -path Dockerfile -Raw) -replace $baseImageName, 'REPLACEWITHBASEIMAGENAME') | Set-Content -Path Dockerfile
    
    Write-Host "$(Get-TimeStamp) End $($dir.Name)"
    Pop-Location
}

Remove-PSDrive -Name GETModels -Force

docker ps -a -q | % { docker stop $_ }
docker ps -a -q | % { docker rm $_ }
"y" | docker image prune
"y" | docker volume prune
docker logout $registryUrl