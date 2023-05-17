New-Item -ItemType Directory -Force -Path C:\DockerImages\Models
Push-Location c:\DockerImages\Models
git init
git remote add -f origin https://cloudolssonassociates.visualstudio.com/DefaultCollection/GET/_git/GET-Models
git config core.sparseCheckout true
"/Containers`n!/Containers/**/SampleInputFiles/" | out-file -encoding ascii .git/info/sparse-checkout
git pull origin master
Pop-Location