# registry credentials for QA
$registryUrl = "getqa.azurecr.io"
$registryUsername = "getqa"
$registryPassword = ""

# image name. update as appropriate
$imagename = "adjustcohystrch"

docker-compose build

docker login -u $registryUsername -p $registryPassword  $registryUrl
docker tag $imagename $registryUrl/$imagename
docker push $registryUrl/$imagename