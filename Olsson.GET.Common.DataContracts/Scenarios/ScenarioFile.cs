namespace Olsson.GET.Common.DataContracts.Scenarios
{
    public class ScenarioFile
    {
        public int ScenarioFileID { get; set; }

        public int ScenarioID { get; set; }

        public string ScenarioFileName { get; set; }

        public string ScenarioFileDescription { get; set; }

        public bool IsRequired { get; set; }

        public bool Uploaded { get; set; }

        public Scenario Scenario { get; set; }
    }
}
