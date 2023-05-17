This folder contains an example on how to create a docker image for custom input parsing that uses an R script.

To create a docker image locally:
1. Create a Dockerfile that sets up the image (installing libraries, adding files, etc). 
2. Create a docker-compose.yml file with the desired image name.
3. Run "docker-compose build" in command line (e.g. Powershell)

For local testing:
The environment variables are supplied in the C# code when creating Azure Container Instances. However, if running locally, the 'ENV' declarations can be uncommented.

To mount a volume when running a docker instance, run the following command:
docker run -v <your_source_folder>:/<target_folder_in_container> <imagename>

In this example, a csv file is expected. That csv file should be in the volume to be mounted. In Azure, the volume will be an Azure File Storage folder.

E.g. 
my local source folder: C:\Users\ylee\Downloads\RScripts
target folder: input
image name: adjustcohystrch
Command:
docker run -v C:\Users\ylee\Downloads\RScripts:/input adjustcohystrch

Notes:
All R libraries used in the R script need to be setup in the Docker environment. 

The generate input program is also responsible in making a HTTP call to trigger the next step of the process. (the commented out ngrok url will likely have expired and no longer valid.)

To push the images to Azure:
1. Create the docker image locally as specified above
2. update the "push_image.ps1" file with the image name to field "$imagename"
3. run "push_image.ps1" in powershell