using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ContainerInstance;
using Azure.ResourceManager.ContainerInstance.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Olsson.GET.Common.DataContracts.Container;
using Olsson.GET.Common.Shared.Enums;
using Olsson.GET.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ContainerEvent = Olsson.GET.Common.DataContracts.Container.ContainerEvent;

namespace Olsson.GET.Accessors.Containers
{
    class ContainerAccessor : BaseTableAccessor, IContainerAccessor
    {
        private const string CreatedByLabelKey = "Created By";
        private const string CreatedByLabelValue = "Olsson GET Container Accessor";
        private string[] ContainerStatusesNotStart = new string[] { "Creating", "Running", "Failed", "Stopped" };
        private static readonly ILogger Logger = Logging.GetLogger<ContainerAccessor>();
        private AzureLocation azureRegion = AzureLocation.CentralUS;

        static ContainerAccessor()
        {
            //need this to pass validation on the self signed cert
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
        }

        public void StartAzureContainer(string fileLocator,
            string imageName,
            double cpuCoreSize,
            double memory,
            Dictionary<string, string> envVars,
            AgentProcessType processType,
            bool isLinux = false)
        {
            var resourceGroupName = ConfigurationHelper.AppSettings.AzureResourceGroup;

            var containerGroupName = $"{fileLocator}-{processType.ToString().ToLower()}";

            // Authenticate with Azure
            var azure = GetAzureContext();
            Logger.LogInformation("Authenticated with Azure");
            if (CanCreateContainer(azure, containerGroupName))
            {
                if (isLinux)
                {
                    Logger.LogInformation("Attempting to run Linux based container");
                    RunTaskBasedLinuxContainer(azure,
                        resourceGroupName,
                        containerGroupName,
                        imageName,
                        fileLocator,
                        cpuCoreSize,
                        memory,
                        envVars
                    );
                }
                else
                {
                    Logger.LogInformation("Attempting to run Windows based container");
                    RunTaskBasedWindowsContainer(azure,
                        containerGroupName,
                        imageName,
                        cpuCoreSize,
                        memory,
                        envVars
                    );
                }
            }
        }

        public async Task<List<ExitedContainer>> GetAzureContainers()
        {
            var azure =  GetAzureContext();
            var resourceGroup = (await azure.GetResourceGroupAsync(ConfigurationHelper.AppSettings.AzureResourceGroup)).Value;
            var resourceGroupContainerGroups = resourceGroup.GetContainerGroups().GetAll().ToList();

            return GetAzureContainers(resourceGroupContainerGroups);
        }

        public async Task<List<ExitedContainer>> GetAllAzureContainers()
        {
            var azure = GetAzureContext();
            var allContainersPageable = azure.GetContainerGroupsAsync();

            var allContainers = new List<ContainerGroupResource>();
            await foreach (var containerPage in allContainersPageable)
            {
                allContainers.Add(containerPage);
            }

            return GetAzureContainers(allContainers.Where(a => a.Data.Location == azureRegion).ToList());
        }

        private List<ExitedContainer> GetAzureContainers(IEnumerable<ContainerGroupResource> containerGroups)
        {
            var exitedContainers = new List<ExitedContainer>();

            foreach (var containerGroup in containerGroups)
            {
                var containerGroupInstance = containerGroup.Get().Value;

                var firstContainerInContainerGroup = containerGroupInstance.Data.Containers.First();
                var newExitedContainer = new ExitedContainer
                {
                    Id = containerGroupInstance.Id,
                    GroupName = containerGroupInstance.Data.Name,
                    ContainerName = firstContainerInContainerGroup.Name,
                    State = containerGroupInstance.Data.InstanceView.State,
                    Events = firstContainerInContainerGroup.InstanceView?.Events != null &&
                             firstContainerInContainerGroup.InstanceView.Events.Any()
                        ? containerGroupInstance.Data.Containers.First().InstanceView.Events.Select(x => new ContainerEvent
                        {
                            LastTimeStamp = x.LastTimestamp?.DateTime,
                            Message = x.Message,
                            Name = x.Name
                        }).ToList()
                        : null
                };
                exitedContainers.Add(newExitedContainer);
            }

            return exitedContainers;
        }

        public async Task DeleteAzureContainer(string id)
        {
            Logger.LogInformation($"Removing container [{id}]");
            var azure = GetAzureContext();
            var resourceGroup = (await azure.GetResourceGroupAsync(ConfigurationHelper.AppSettings.AzureResourceGroup)).Value;
            var containerGroup = resourceGroup.GetContainerGroups().GetAll().Single(x => x.Id == id);
            await containerGroup.DeleteAsync(WaitUntil.Started);
        }

        public async Task RestartContainerAsync(string id)
        {
            var azure = GetAzureContext();
            var resourceGroup = (await azure.GetResourceGroupAsync(ConfigurationHelper.AppSettings.AzureResourceGroup)).Value;
            var containerGroup = resourceGroup.GetContainerGroups().GetAll().Single(x => x.Id == id);
            await containerGroup.StartAsync(WaitUntil.Started);
        }

        public async Task StopContainerAsync(string id)
        {
            var azure = GetAzureContext();
            var resourceGroup = (await azure.GetResourceGroupAsync(ConfigurationHelper.AppSettings.AzureResourceGroup)).Value;
            var containerGroup = resourceGroup.GetContainerGroups().GetAll().Single(x => x.Id == id);
            
            await containerGroup.StopAsync();
        }

        private SubscriptionResource GetAzureContext()
        {
            var credential = new ClientSecretCredential(ConfigurationHelper.AppSettings.FunctionTenantId, ConfigurationHelper.AppSettings.FunctionClientId, ConfigurationHelper.AppSettings.FunctionSecret);
            ArmClient client = new ArmClient(credential);

            var defaultSubscription = client.GetDefaultSubscription();
            return defaultSubscription;
        }

        private void RunTaskBasedWindowsContainer(SubscriptionResource azure,
           string containerGroupName,
           string containerImage,
           double cpuCoreSize,
           double memory,
           Dictionary<string, string> envVars)
        {
            var registryServer = ConfigurationHelper.AppSettings.AzureRegistryServer;
            var registryUsername = ConfigurationHelper.AppSettings.AzureRegistryUsername;
            var registryPassword = ConfigurationHelper.AppSettings.AzureRegistryPassword;

            var containerInstanceName = $"{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
            Logger.LogInformation($"Attempting to run container with instance name {containerInstanceName}");
            var resourceGroup = azure.GetResourceGroup(ConfigurationHelper.AppSettings.AzureResourceGroup).Value;
            
            
            var containerEnvironmentVariables = envVars.Select(x => new ContainerEnvironmentVariable(x.Key)
            {
                Value = x.Value
            }).ToList();


            var containerInstanceContainers = new List<ContainerInstanceContainer>()
            {
                new(
                    containerInstanceName,
                    $"{ConfigurationHelper.AppSettings.AzureRegistryServer}/{containerImage}:latest",
                    new ContainerResourceRequirements(new ContainerResourceRequestsContent(memory, cpuCoreSize))
                )
                {
                    Ports = { new ContainerPort(int.Parse(ConfigurationHelper.AppSettings.AzureContainerTcpPort)) }
                }
            };

            containerInstanceContainers.ForEach(container =>
            {
                containerEnvironmentVariables.ForEach(envVar =>
                {
                    container.EnvironmentVariables.Add(envVar);
                });
            });
            Logger.LogInformation($"Attempting to CreateOrUpdate container group \"{containerGroupName}\"");


            var containerGroup =  resourceGroup.GetContainerGroups()
                .CreateOrUpdateAsync(
                    WaitUntil.Completed,
                    containerGroupName,
                    new ContainerGroupData(azureRegion, containerInstanceContainers, ContainerInstanceOperatingSystemType.Windows)
                    {
                        ImageRegistryCredentials = { 
                            new ContainerGroupImageRegistryCredential(registryServer)
                            {
                                Username = registryUsername,
                                Password = registryPassword
                            }
                        },
                        RestartPolicy = ContainerGroupRestartPolicy.Never,
                        IPAddress = { 
                            AddressType = ContainerGroupIPAddressType.Private, 
                            Ports = { new ContainerGroupPort(int.Parse(ConfigurationHelper.AppSettings.AzureContainerTcpPort)) },
                            DnsNameLabel = containerGroupName
                        },
                    }).Result.Value;
            
            // Print the container's logs
            Console.WriteLine($"Logs for container '{containerInstanceName}':");
            Console.WriteLine(containerGroup.GetContainerLogs(containerInstanceName).Value.Content);
        }

        private void RunTaskBasedLinuxContainer(SubscriptionResource azure,
            string resourceGroupName,
            string containerGroupName,
            string containerImage,
            string fileLocator,
            double cpuCoreSize,
            double memory,
            Dictionary<string, string> envVars)
        {


            var registryServer = ConfigurationHelper.AppSettings.AzureRegistryServer;
            var registryUsername = ConfigurationHelper.AppSettings.AzureRegistryUsername;
            var registryPassword = ConfigurationHelper.AppSettings.AzureRegistryPassword;

            var containerInstanceName = $"{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";

            var resourceGroup = azure.GetResourceGroup(ConfigurationHelper.AppSettings.AzureResourceGroup).Value;

            var containerEnvironmentVariables = envVars.Select(x => new ContainerEnvironmentVariable(x.Key)
            {
                Value = x.Value
            }).ToList();


            var containerInstanceContainers = new List<ContainerInstanceContainer>()
            {
                new(
                    containerInstanceName,
                    $"{ConfigurationHelper.AppSettings.AzureRegistryServer}/{containerImage}:latest",
                    new ContainerResourceRequirements(new ContainerResourceRequestsContent(memory, cpuCoreSize))
                )
                {
                    Ports = { new ContainerPort(int.Parse(ConfigurationHelper.AppSettings.AzureContainerTcpPort)) },
                    VolumeMounts = { new ContainerVolumeMount(fileLocator, $"/{ConfigurationHelper.AppSettings.AzureContainerVolumeName}/") }
                }
            };

            containerInstanceContainers.ForEach(container =>
            {
                containerEnvironmentVariables.ForEach(envVar =>
                {
                    container.EnvironmentVariables.Add(envVar);
                });
            });

            var containerGroup = resourceGroup.GetContainerGroups()
                .CreateOrUpdateAsync(
                    WaitUntil.Completed,
                    containerGroupName,
                    new ContainerGroupData(azureRegion, containerInstanceContainers, ContainerInstanceOperatingSystemType.Linux)
                    {
                        ImageRegistryCredentials = 
                        {
                            new ContainerGroupImageRegistryCredential(registryServer)
                            {
                                Username = registryUsername,
                                Password = registryPassword
                            }
                        },
                        Volumes = 
                        {
                            new ContainerVolume(fileLocator)
                            {
                                AzureFile = new ContainerInstanceAzureFileVolume(fileLocator, ConfigurationHelper.AppSettings.AzureStorageAccountName)
                                {
                                    StorageAccountKey = ConfigurationHelper.AppSettings.AzureStorageAccountKey
                                }
                            }
                        },
                        RestartPolicy = ContainerGroupRestartPolicy.Never,
                        IPAddress = 
                        {
                            AddressType = ContainerGroupIPAddressType.Private,
                            Ports = { new ContainerGroupPort(int.Parse(ConfigurationHelper.AppSettings.AzureContainerTcpPort)) },
                            DnsNameLabel = containerGroupName
                        },
                    }).Result.Value;

            // Print the container's logs
            Console.WriteLine($"Logs for container '{containerInstanceName}':");
            Console.WriteLine(containerGroup.GetContainerLogs(containerInstanceName).Value.Content);
        }

        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public async Task<bool> CanQueueNewContainer()
        {
            //we may want to move to a "CloudLock"
            //https://lixar.com/lixar-blog/tech/concurrency-control-locking-microsoft-azure/
            //https://github.com/lixar/Lixar.Azure/blob/master/Lixar.Azure/Storage/CloudLock.cs
            await semaphoreSlim.WaitAsync();
            try
            {
                var containers = await GetAllAzureContainers();
                var maxContainerCount = ConfigurationHelper.AppSettings.MaxContainerCount;
                var containerCount = containers.Count;

                Logger.LogInformation($"Max Container Count: {maxContainerCount}");
                Logger.LogInformation($"Current Container Count: {containerCount}");
                if (containerCount < maxContainerCount)
                {
                    return true;
                }

                Logger.LogInformation("Out of available containers.  Searching to see if some successful containers can be deleted.");
                var deleteContainerTasks = containers.Where(a => a.State.Equals("Succeeded"))
                                                   .OrderBy(a => a.Events?.Max(b => b.LastTimeStamp) ?? DateTime.MaxValue)
                                                   .Select(CleanupContainer);

                var deleteResults = await Task.WhenAll(deleteContainerTasks);

                var deletedContainerCount = deleteResults.Count(a => a);
                containerCount -= deletedContainerCount;

                Logger.LogInformation($"Deleted {deletedContainerCount} containers, leaving {containerCount} containers.");

                var canQueueNewContainer = containerCount < maxContainerCount;

                if (!canQueueNewContainer)
                {
                    Logger.LogWarning("Out of available containers.  Unable to start a new container.");
                }

                return canQueueNewContainer;
            }
            catch (Exception ex)
            {
                //this seems to happen pretty commonly when Staging and Prod are fighting for containers.
                Logger.LogWarning("An exception occured trying to get the containers.", ex);
                return false;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private async Task<bool> CleanupContainer(ExitedContainer container)
        {
            try
            {
                await DeleteAzureContainer(container.Id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanCreateContainer(SubscriptionResource azure,
            string containerGroupName)
        {
            Logger.LogInformation($"Checking if we can create a container with group name {containerGroupName}.");
            var resourceGroup = azure.GetResourceGroup(ConfigurationHelper.AppSettings.AzureResourceGroup).Value;
            var allContainerGroups = resourceGroup.GetContainerGroups();
            
            if (allContainerGroups.Exists(containerGroupName))
            {
                var containerGroup = allContainerGroups.Get(containerGroupName).Value;
                return !ContainerStatusesNotStart.Contains(containerGroup.Data.InstanceView.State);
            }

            return true;
        }
    }
}
