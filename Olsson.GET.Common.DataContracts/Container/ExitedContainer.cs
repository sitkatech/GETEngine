using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Olsson.GET.Common.DataContracts.Container
{
    [DataContract]
    public class ExitedContainer
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string GroupName { get; set; }
        [DataMember]
        public string ContainerName { get; set; }
        [DataMember]
        public string State { get; set; }
        [DataMember]
        public string Log { get; set; }
        public List<ContainerEvent> Events { get; set; }
    }

    public class ContainerEvent
    {
        public string Name { get; set; }

        public string Message { get; set; }

        public DateTime? LastTimeStamp { get; set; }
    }
}
