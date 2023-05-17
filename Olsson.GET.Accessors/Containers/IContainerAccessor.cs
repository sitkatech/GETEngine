using System.Collections.Generic;
using System.Threading.Tasks;
using Olsson.GET.Common.DataContracts.Container;
using Olsson.GET.Common.Shared.Enums;

namespace Olsson.GET.Accessors.Containers
{
    public interface IContainerAccessor
    {
        void StartAzureContainer(string fileLocator, string imageName, double cpuCoreSize, double memory, Dictionary<string, string> envVars, AgentProcessType processType, bool isLinux = false);
        Task<List<ExitedContainer>> GetAzureContainers();
        Task DeleteAzureContainer(string id);
        Task RestartContainerAsync(string id);
        Task StopContainerAsync(string id);
        Task<bool> CanQueueNewContainer();
    }
}
