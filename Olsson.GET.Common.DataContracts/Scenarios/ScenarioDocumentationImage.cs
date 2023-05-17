using Olsson.GET.Common.DataContracts.FileResource;

namespace Olsson.GET.Common.DataContracts.Scenarios
{
    public class ScenarioDocumentationImage
    {
        public int ScenarioDocumentationImageID { get; set; }
        public int ScenarioID { get; set; }
        public int FileResourceInfoID { get; set; }
        public Scenario Scenario { get; set; }
        public FileResourceInfo FileResourceInfo { get; set; }
    }
}