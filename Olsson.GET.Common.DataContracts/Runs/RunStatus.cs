using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace Olsson.GET.Common.DataContracts.Runs
{
    [DataContract]
    public class RunStatus
    {
        [DataMember]
        public int RunStatusID { get; set; }
        [DataMember]
        public string RunStatusName { get; set; }
        [DataMember]
        public string RunStatusDisplayName { get; set; }
        [DataMember]
        public string RunStatusColor { get; set; }
        [DataMember]
        public bool IsTerminal { get; set; }

    }
}
