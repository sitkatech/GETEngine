using log4net;

using Olsson.GET.Common.DataContracts.Container;
using Olsson.GET.Common.Shared.Enums;
using Olsson.GET.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace Olsson.GET.Accessors.Containers
{
    class ContainerAccessor : BaseTableAccessor, IContainerAccessor
    {
        private const string CreatedByLabelKey = "Created By";
        private const string CreatedByLabelValue = "Olsson GET Container Accessor";
        private string[] ContainerStatusesNotStart = new string[] { "Creating", "Running", "Failed", "Stopped" };
        private static readonly ILog Logger = Logging.GetLogger(typeof(ContainerAccessor));
        private Region azureRegion = Region.USCentral;

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
            IAzure azure = GetAzureContext();

            if (CanCreateContainer(azure, containerGroupName))
            {
                if (isLinux)
                {
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
                    RunTaskBasedWindowsContainer(azure,
                        resourceGroupName,
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
            var resourceGroupContainerGroups = await GetAzureContext().ContainerGroups.ListByResourceGroupAsync(ConfigurationHelper.AppSettings.AzureResourceGroup, true);
            return GetAzureContainers(resourceGroupContainerGroups);
        }

        public async Task<List<ExitedContainer>> GetAllAzureContainers()
        {
            var allContainers = await GetAzureContext().ContainerGroups.ListAsync(true);
            return GetAzureContainers(allContainers.Where(a => a.Region == azureRegion));
        }

        private List<ExitedContainer> GetAzureContainers(IEnumerable<IContainerGroup> containerGroups)
        {
            var exitedContainers = new List<ExitedContainer>();

            foreach (var containerGroup in containerGroups)
            {
                var firstContainerInContainerGroup = containerGroup.Containers.First();
                exitedContainers.Add(new ExitedContainer
                {
                    Id = containerGroup.Id,
                    GroupName = containerGroup.Name,
                    ContainerName = firstContainerInContainerGroup.Key,
                    State = containerGroup.Inner.InstanceView.State,
                    Events = firstContainerInContainerGroup.Value.InstanceView?.Events != null && firstContainerInContainerGroup.Value.InstanceView.Events.Any() ?
                        containerGroup.Containers.First().Value.InstanceView.Events.Select(x => new ContainerEvent
                        {
                            LastTimeStamp = x.LastTimestamp,
                            Message = x.Message,
                            Name = x.Name
                        }).ToList() :
                        null
                });
            }

            return exitedContainers;
        }

        public async Task DeleteAzureContainer(string id)
        {
            Logger.Info($"Removing container [{id}]");
            var azure = GetAzureContext();
            await azure.ContainerGroups.DeleteByIdAsync(id);
        }

        public async Task RestartContainerAsync(string id)
        {
            var azure = GetAzureContext();

            var containerGroup = azure.ContainerGroups.GetById(id);

            await ContainerGroupsOperationsExtensions.StartAsync(
                    containerGroup.Manager.Inner.ContainerGroups,
                    containerGroup.ResourceGroupName,
                    containerGroup.Name);
        }

        public async Task StopContainerAsync(string id)
        {
            var azure = GetAzureContext();

            var containerGroup = azure.ContainerGroups.GetById(id);

            await ContainerGroupsOperationsExtensions.StopAsync(
                    containerGroup.Manager.Inner.ContainerGroups,
                    containerGroup.ResourceGroupName,
                    containerGroup.Name);
        }

        private IAzure GetAzureContext()
        {
            IAzure azure = null;

            var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(
                   ConfigurationHelper.AppSettings.FunctionClientId,
                    ConfigurationHelper.AppSettings.FunctionSecret,
                    ConfigurationHelper.AppSettings.FunctionTenantId,
                    AzureEnvironment.AzureGlobalCloud);

            azure = Microsoft.Azure.Management.Fluent.Azure.Authenticate(credentials).WithDefaultSubscription();

            return azure;
        }

        private void RunTaskBasedWindowsContainer(IAzure azure,
           string resourceGroupName,
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

            // Create the container group
            var containerGroup = azure.ContainerGroups.Define(containerGroupName)
                .WithRegion(azureRegion)
                .WithExistingResourceGroup(resourceGroupName)
                .WithWindows()
                .WithPrivateImageRegistry(registryServer, registryUsername, registryPassword)
                .WithoutVolume()
                .DefineContainerInstance(containerInstanceName)
                    .WithImage($"{ConfigurationHelper.AppSettings.AzureRegistryServer}/{containerImage}:latest")
                    .WithExternalTcpPort(int.Parse(ConfigurationHelper.AppSettings.AzureContainerTcpPort))
                    .WithCpuCoreCount(cpuCoreSize)
                    .WithMemorySizeInGB(memory)
                    .WithEnvironmentVariables(envVars)
                    .Attach()
                .WithDnsPrefix(containerGroupName)
                .WithRestartPolicy(ContainerGroupRestartPolicy.Never)
                .Create();

            // Print the container's logs
            Console.WriteLine($"Logs for container '{containerInstanceName}':");
            Console.WriteLine(containerGroup.GetLogContent(containerInstanceName));
        }

        private void RunTaskBasedLinuxContainer(IAzure azure,
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

            // Create the container group
            var containerGroup = azure.ContainerGroups.Define(containerGroupName)
                .WithRegion(azureRegion)
                .WithExistingResourceGroup(resourceGroupName)
                .WithLinux()
                .WithPrivateImageRegistry(registryServer, registryUsername, registryPassword)
                .DefineVolume(fileLocator)
                    .WithExistingReadWriteAzureFileShare(fileLocator)
                    .WithStorageAccountName(ConfigurationHelper.AppSettings.AzureStorageAccountName)
                    .WithStorageAccountKey(ConfigurationHelper.AppSettings.AzureStorageAccountKey)
                    .Attach()
                .DefineContainerInstance(containerInstanceName)
                    .WithImage($"{ConfigurationHelper.AppSettings.AzureRegistryServer}/{containerImage}:latest")
                    .WithExternalTcpPort(int.Parse(ConfigurationHelper.AppSettings.AzureContainerTcpPort))
                    .WithVolumeMountSetting(fileLocator, $"/{ConfigurationHelper.AppSettings.AzureContainerVolumeName}/")
                    .WithCpuCoreCount(cpuCoreSize)
                    .WithMemorySizeInGB(memory)
                    .WithEnvironmentVariables(envVars)
                    .Attach()
                .WithDnsPrefix(containerGroupName)
                .WithRestartPolicy(ContainerGroupRestartPolicy.Never)
                .Create();

            // Print the container's logs
            Console.WriteLine($"Logs for container '{containerInstanceName}':");
            Console.WriteLine(containerGroup.GetLogContent(containerInstanceName));
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

                Logger.Info($"Max Container Count: {maxContainerCount}");
                Logger.Info($"Current Container Count: {containerCount}");
                if (containerCount < maxContainerCount)
                {
                    return true;
                }

                Logger.Info("Out of available containers.  Searching to see if some successful containers can be deleted.");
                var deleteContainerTasks = containers.Where(a => a.State.Equals("Succeeded"))
                                                   .OrderBy(a => a.Events?.Max(b => b.LastTimeStamp) ?? DateTime.MaxValue)
                                                   .Select(CleanupContainer);

                var deleteResults = await Task.WhenAll(deleteContainerTasks);

                var deletedContainerCount = deleteResults.Count(a => a);
                containerCount -= deletedContainerCount;

                Logger.Info($"Deleted {deletedContainerCount} containers, leaving {containerCount} containers.");

                var canQueueNewContainer = containerCount < maxContainerCount;

                if (!canQueueNewContainer)
                {
                    Logger.Warn("Out of available containers.  Unable to start a new container.");
                }

                return canQueueNewContainer;
            }
            catch (Microsoft.Rest.Azure.CloudException ex)
            {
                //this seems to happen pretty commonly when Staging and Prod are fighting for containers.
                Logger.Warn("An exception occured trying to get the containers.", ex);
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

        private bool CanCreateContainer(IAzure azure,
            string containerGroupName)
        {
            var containerGroup = azure.ContainerGroups.GetByResourceGroup(ConfigurationHelper.AppSettings.AzureResourceGroup, containerGroupName);

            if (containerGroup != null)
            {
                return !ContainerStatusesNotStart.Contains(containerGroup.Inner.InstanceView.State);
            }

            return true;
        }
    }
}
