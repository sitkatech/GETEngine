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
        public List<UserDataPoint> UserDataPointInputs { get; set; }
    }

    [DataContract]
    public class UserDataPoint
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
        public List<UserDataPointTimeStep> TimeSteps { get; set; }
    }

    public class UserDataPointTimeStep
    {
        public DateTime DateTime { get; set; }
        public double Value { get; set; }
    }


}