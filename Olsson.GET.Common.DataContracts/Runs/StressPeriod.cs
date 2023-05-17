using System.Runtime.Serialization;

namespace Olsson.GET.Common.DataContracts.Runs
{
    [DataContract]
    public class StressPeriod
    {
        [DataMember]
        public double Days { get; set; }

        [DataMember]
        public int NumberOfTimeSteps { get; set; }
    }
}