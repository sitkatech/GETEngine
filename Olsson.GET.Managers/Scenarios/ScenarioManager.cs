using Serilog;
using Olsson.GET.Accessors.Scenarios;
using Olsson.GET.Common.DataContracts.Scenarios;
using Olsson.GET.Common.Utilities;
using System.Collections.Generic;

namespace Olsson.GET.Managers.Scenarios
{
    public class ScenarioManager : BaseManager, IScenarioManager
    {
        private static readonly ILogger Logger = Logging.GetLogger<ScenarioManager>();

        public List<Scenario> List()
        {
            Logger.Information("Finding all scenarios");

            return AccessorFactory.CreateAccessor<IScenarioAccessor>().List();
        }

        public List<Scenario> ListForCustomerId(int customerId)
        {
            Logger.Information($"Finding all scenarios for customerId:{customerId}");

            return AccessorFactory.CreateAccessor<IScenarioAccessor>().ListForCustomerId(customerId);
        }

        public Scenario GetById(int id)
        {
            Logger.Information($"Finding scenario with id:{id}");

            return AccessorFactory.CreateAccessor<IScenarioAccessor>().GetById(id);
        }

        public bool ChangeScenarioDescription(int id, string newDescription)
        {
            Logger.Information($"Updating description for scenario with id:{id}");

            return AccessorFactory.CreateAccessor<IScenarioAccessor>().ChangeScenarioDescription(id, newDescription);
        }

        public bool ChangeShowToAllUsersInScenarioList(int id, bool showToAllUsersInScenarioList)
        {
            Logger.Information($"Updating ShowToAllUsersInScenarioList for scenario with id:{id}");

            return AccessorFactory.CreateAccessor<IScenarioAccessor>().ChangeShowToAllUsersInScenarioList(id, showToAllUsersInScenarioList);
        }

        public bool UpdateScenarioDocumentation(int id, string newDocumentation)
        {
            Logger.Information($"Updating Scenario Documentation for scenario with id:{id}");

            return AccessorFactory.CreateAccessor<IScenarioAccessor>().UpdateScenarioDocumentation(id, newDocumentation);

        }

        public bool CreateScenarioDocumentationImage(int scenarioId, int fileResourceInfoId)
        {
            Logger.Information($"Creating Scenario Documentation Image for scenario with id:{scenarioId} and file resource info with id:{fileResourceInfoId}");

            return AccessorFactory.CreateAccessor<IScenarioAccessor>().CreateScenarioDocumentationImage(scenarioId, fileResourceInfoId);
        }
    }
}