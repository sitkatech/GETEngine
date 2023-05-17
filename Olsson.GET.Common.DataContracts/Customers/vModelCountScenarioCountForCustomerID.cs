using System.Runtime.Serialization;

namespace Olsson.GET.Common.DataContracts.Customers
{
    public class vModelCountScenarioCountForCustomerID
    {
        [DataMember]
        public int CustomerID { get; set; }

        [DataMember]
        public int ModelCount { get; set; }

        [DataMember]
        public int ScenarioCount { get; set; }

    }
}