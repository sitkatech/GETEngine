using System;
using System.Runtime.Serialization;
using Olsson.GET.Common.DataContracts.Users;

namespace Olsson.GET.Common.DataContracts.FileResource
{
    public class FileResourceInfo
    {
        [DataMember]
        public int FileResourceInfoID { get; set; }

        [DataMember]
        public string OriginalBaseFilename { get; set; }

        [DataMember]
        public string OriginalFileExtension { get; set; }

        [DataMember]
        public Guid FileResourceGUID { get; set; }
        
        [DataMember]
        public DateTime CreateDate { get; set; }

        [DataMember]
        public User User { get; set; }

        [DataMember]
        public FileResourceMimeType FileResourceMimeType { get; set; }

        public string GetFileResourceGUIDAsString()
        {
            return FileResourceGUID.ToString();
        }

    }
}