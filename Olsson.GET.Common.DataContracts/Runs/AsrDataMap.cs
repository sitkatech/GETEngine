using Olsson.GET.Common.DataContracts.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Olsson.GET.Common.DataContracts.Runs
{
    [DataContract]
    public class AsrDataMap
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    public class LocationPumpingProportion
    {
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public double Proportion { get; set; }
    }

    public class LocationWithBounds
    {
        [DataMember]
        public string Location { get; set; }

        [DataMember]
        public List<Coordinate> BoundCoordinates { get; set; }
    }

    [DataContract]
    public class ZoneBudgetItem
    {
        [DataMember]
        public int Period { get; set; }
        [DataMember]
        public int Step { get; set; }
        [DataMember]
        public string Zone { get; set; }
        [DataMember]
        public List<ZoneBudgetValue> Values { get; set; }
    }

    [DataContract]
    public class ZoneBudgetValue
    {
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public double Value { get; set; }
    }
}