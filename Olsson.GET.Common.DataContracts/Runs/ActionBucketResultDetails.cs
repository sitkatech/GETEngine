using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Olsson.GET.Common.DataContracts.Runs
{
    [DataContract]
    public class ActionBucketResultDetails
    {
        [DataMember]
        public List<RunResultSet> ResultSets { get; set; }

        [DataMember]
        public List<ActionBucketRelatedResultOption> RelatedResultOptions { get; set; }
    }

    [DataContract]
    public class ActionBucketRelatedResultOption
    {
        [DataMember]
        public int ResultId { get; set; }

        [DataMember]
        public string FileStorageLocator { get; set; }

        [DataMember]
        public string RelatedResultName { get; set; }
    }
}
