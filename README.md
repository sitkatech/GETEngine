# Project Summary
The Olsson GET application provides an interface for running Modflow and Modpath simulations.

GET exists to
- trigger Modflow and Modpath runs to start
- store the results of these runs in Azure Blob storage
- allow reading and interpreting of the results of these runs

The basic workflow of GET is:
- Create actions/runs
- Generate the Modflow/Modpath inputs for the action/run 
- Run the analysis for the action/run
- Generate outputs for the action/run
- View outputs (i.e. graphs, data) for the action/run

# Tooling

This application uses the following tools:
- Visual Studio 2019
- SQL Server & SSMS
- Azure Blob Storage
- Docker

# Setup

## Configuration

1. Install the SlowCheetah Extension from the Visual Studio Extensions Menu

## Database

1. Run DbUp for your local database
    a. In Visual Studio, set Primary (under 5.Databases) as the startup project
2. Run DbUp for your unit test database
    a. Change the connection string to use the initial catalog "Get.Primary.Tests"
    b. After you run it, undo the change (back to "Get.Primary")

## Blob Storage

1. Run Azure Storage Emulator to simulate blobs and queues locally
    - You can manipulate these directly with Azure Storage Explorer

# Running the Application Locally

## Test Projects

Make sure you've setup your test database (see previous section).

CAUTION: Avoid stopping tests in the middle of execution. The tests will not gracefully free up resources.
If you get in this scenario, restart visual studio to manually free things back up.

It is highly recommended to develop against tests as much as possible since the models can take quite a long time to run.
You can even point tests to your local Model folder to test against without running the time-consuming local process.

## Portal

The portal project is found under 1.Clients and can be run like a normal MVC application.

You can generate your initial credentials by starting up the project and then going to:

    localhost:61898/Account/CreateDefaultUser

This will create the following admin user.

    username: smurtagh@dontpaniclabs.com
    password: D0ntp@n1c

## Enabling your Models

1. In File Explorer, access the models directory here: \\getqa.file.core.windows.net\models\Containers
    a. Credentials are:
        u: Azure\getqa
        p: 
2. Make two new folders C:\GETModels and C:\Model
    a. The first is just a convenient place to store all of your local models and the second is a working directory for your Agent process later.
3. Drop cpnrd, cpnrd50yr, and cpnrdcohyst into your new Models folder.
    a. CPNRD and CPNRD 50 Year run fast because they cover a small geological area
    b. CPNRD COHYST has good variety for testing.
4. Take this moment to copy the /Model folder from one of your models into that C:\Model working directory. (Probably go with the C:\GETModels\cpnrd\Model files)
5. Enable models in the portal UI
    a. Go to Customers
    b. Go to Admin
    c. Click the checkboxes for the models you want to enable
        - Central Platt NRD, Central Platt NRD - 50 Year, and Cohyst
    d. Remember to save at the bottom (NOT at the top of the page)

## Running a Model

Normally (i.e. in the server environments), there is an orchestration process with queues that triggers the agent processes and
moves the action/run between states appropriately. To do this locally requires running the Agent process manually with command line arguments.
The process is a little bit janky, but you won't have to run the Orchestrator or ApiFunctions.

**In the Portal**

1. Create a NEW ACTION
2. Select your model and other relevant output and save
3. Add a well by clicking on the map, save that
4. Process With Maps

**Run the Agent (Either via command line or Visual Studio)**

0. If you didn't use a separate C:/GETModels and C:/Model folder, you have to change your Agent config to point to the folder with your model files.

    <add key="ModflowDataFolder" value="C:\Model\cpnrd\Model"/> (for example)

But if you set it up how we recommended above, you already have your model files in "C:\Model". You're already good.

1. Grab the ID of your action out of the URL in the portal

2. Run {ID} 1 to process inputs. This will modify/generate a .WEL file in your Model folder.

    NOTE: If you don't have the API Functions running, this will fail the manager to manager call and error out.
    This is okay because we aren't using the orchestration locally. As long as you get your .WEL file created/modified, you can move on.

3. Run {ID} 2 to run analysis. This will take 10-15 minutes. This will run the analysis and then trigger outputs to be generated.
If you have the portal running concurrently, you will see the action go to Analysis Succeeded and eventually to Completed.
You can confirm that files were created/modified in your Model folder to know that the analysis worked.

**View Run/Action Outputs**
Go back to the action details to view graphs & raw data.