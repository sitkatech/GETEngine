using System.Collections.Generic;
using System.Linq;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Runs;

namespace Olsson.GET.Common.DataContracts.Scenarios
{
    public static partial class ScenarioExtensionMethods
    {
        public static List<ModelSimpleDto> GetCustomerModelsForScenario(this Scenario scenario, int customerID)
        {
            return scenario.Models.Where(x => scenario.CustomerModelScenarios.Any(y => y.ModelID == x.ModelID && y.CustomerID == customerID)).OrderBy(x => x.ModelName).ToList();
        }

        public static List<RunSimpleDto> GetCustomerRunsForScenario(this Scenario scenario, int customerID)
        {
            return scenario.Runs.Where(x => x.CustomerID == customerID).OrderByDescending(x => x.CreatedDate).ToList();
        }
    }
}