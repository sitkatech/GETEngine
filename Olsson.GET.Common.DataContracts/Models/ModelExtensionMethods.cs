using System.Collections.Generic;
using System.Linq;
using Olsson.GET.Common.DataContracts.Scenarios;

namespace Olsson.GET.Common.DataContracts.Models
{
    public static partial class ModelExtensionMethods
    {
        public static List<Scenario> GetCustomerScenariosForModel(this Model model, int customerID)
        {
            var scenarioIDsForCustomer = model.CustomerModelScenarios.Where(y => y.CustomerID == customerID)
                .Select(x => x.ScenarioID);
            var scenarios = model.Scenarios.Where(x => scenarioIDsForCustomer.Contains(x.ScenarioID)).OrderBy(x => x.ScenarioName).ToList();
            return scenarios;
        }

        public static ModelTypeEnum GetModelTypeEnumForModel(this Model model)
        {
            return model.NumberOfStressPeriods == 1 ? ModelTypeEnum.SteadyState : ModelTypeEnum.Transient;
        }
    }
}