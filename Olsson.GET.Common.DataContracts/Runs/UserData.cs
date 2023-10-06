using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Olsson.GET.Common.DataContracts.Runs
{
    [DataContract]
    public class UserDataJson
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember(Name = "PivotedRunWellInputs")]
        public List<UserDataPointInput> UserDataPointInputs { get; set; }
    }

    [DataContract]
    public class UserDataPointInput
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public double Lat { get; set; }
        [DataMember]
        public double Lng { get; set; }
        [DataMember]
        public double? AverageValue { get; set; }
        [DataMember]
        public int ClosestNode { get; set; }
        [DataMember]
        public Dictionary<DateTime, double> WaterLevelsByTimestep { get; set; }
    }


}