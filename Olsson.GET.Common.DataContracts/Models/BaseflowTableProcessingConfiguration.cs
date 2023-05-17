namespace Olsson.GET.Common.DataContracts.Models
{
    public class BaseflowTableProcessingConfiguration
    {
        public int BaseflowTableProcessingConfigurationID { get; set; }

        public string BaseflowTableIndicatorRegexPattern { get; set; }

        public int SegmentColumnNum { get; set; }

        public int FlowToAquiferColumnNum { get; set; }

        public int? ReachColumnNum { get; set; }
    }
}