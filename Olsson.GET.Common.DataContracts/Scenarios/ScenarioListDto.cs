using System.Linq;

namespace Olsson.GET.Common.DataContracts.Scenarios
{
    public class ScenarioListDto
    {
        public ScenarioListDto() { }
        public ScenarioListDto(Scenario scenario)
        {
            ScenarioID = scenario.ScenarioID;
            ScenarioName = scenario.ScenarioName;
            ScenarioDescription = scenario.ScenarioDescription;
            ConfiguredModels = scenario.Models == null || scenario.Models.Count == 0 ? "" : string.Join(", ", scenario.Models.Select(x => x.ModelName).OrderBy(x => x));
        }

        public int ScenarioID { get; set; }
        public string ScenarioName { get; set; }

        public string ScenarioDescription { get; set; }

        public string ConfiguredModels { get; set; }
    }
}