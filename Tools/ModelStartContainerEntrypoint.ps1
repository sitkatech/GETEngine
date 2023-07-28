Write-Host Start container

Write-Host Download latest agent build
$agentPath = 'https://' + $env:STORAGE_ACCOUNT + '.file.core.windows.net/agent/' + $env:SAS_TOKEN + '';
& 'C:\\azcopy\\azcopy.exe' copy $agentPath 'C:\\' --recursive

Write-Host Run Agent
& C:\\agent\\Olsson.GET.Clients.Agent.exe $env:RUN_ID $env:PROCESSTYPE