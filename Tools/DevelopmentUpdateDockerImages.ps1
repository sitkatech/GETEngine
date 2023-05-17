Get-ChildItem -Path ..\Containers -Filter dockerfile -Recurse -ErrorAction SilentlyContinue -Force | ForEach-Object {
    $dir = $_.Directory
    Push-Location $dir
	copy Dockerfile Dockerfile.bak
	Remove-Item Dockerfile
	copy ..\..\Tools\DevelopmentDockerfile Dockerfile
	docker build -t $($dir.Name) .
	Remove-Item Dockerfile
	copy Dockerfile.bak Dockerfile
	Remove-Item Dockerfile.bak
    Pop-Location
}