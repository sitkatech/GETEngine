Write-Host Start container

$agentPath = 'https://' + $env:STORAGE_ACCOUNT + '.file.core.windows.net/agent/' + $env:SAS_TOKEN + '';
Write-Host Download latest agent build
& 'C:\\azcopy\\azcopy.exe' copy $agentPath 'C:\\' --recursive

Write-Host Extract latest agent build
& Expand-Archive -LiteralPath 'C:\\agent\\Olsson.GET.Clients.Agent.zip' -DestinationPath 'C:\\agent\\'

Write-Host Run Agent
& C:\\agent\\Olsson.GET.Clients.Agent.exe $env:RUN_ID $env:PROCESSTYPE