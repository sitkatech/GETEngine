using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Olsson.GET.Common.DataContracts.Runs
{
    public class RunBucket
    {
        [DataMember]
        public int RunBucketID { get; set; }

        [DataMember]
        public string RunBucketName { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public int CustomerID { get; set; }

        [DataMember]
        public int UserID { get; set; }

        [DataMember]
        public List<Run> Runs { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember]
        public string RunBucketDescription { get; set; }

        public List<RunResultListItem> AvailableRunResults { get; set; }
    }
}
