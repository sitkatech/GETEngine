using System.Runtime.Serialization;

namespace Olsson.GET.Common.DataContracts.Runs
{
    [DataContract]
    public class OutputData
    {
        [DataMember]
        public int SegmentNumber { get; set; }

        [DataMember]
        public int ReachNumber { get; set; }

        [DataMember]
        public double FlowToAquifer { get; set; }
    }

    [DataContract]
    public class MapOutputData
    {
        [DataMember]
        public int TimeStep { get; set; }

        [DataMember]
        public int StressPeriod { get; set; }

        [DataMember]
        public string Location { get; set; }

        [DataMember]
        public double? Value { get; set; }
    }
}