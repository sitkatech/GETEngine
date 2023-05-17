using System.Runtime.Serialization;

namespace Olsson.GET.Common.DataContracts.FileResource
{
    [DataContract]
    public class FileResourceData
    {
        [DataMember]
        public int FileResourceDataID { get; set; }
        
        [DataMember]
        public byte[] Data { get; set; }

        [DataMember]
        public FileResourceInfo FileResourceInfo { get; set; }
    }
}
