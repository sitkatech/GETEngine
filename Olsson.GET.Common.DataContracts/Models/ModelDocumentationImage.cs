using Olsson.GET.Common.DataContracts.FileResource;

namespace Olsson.GET.Common.DataContracts.Models
{
    public class ModelDocumentationImage
    {
        public int ModelDocumentationImageID { get; set; }
        public int ModelID { get; set; }
        public int FileResourceInfoID { get; set; }
        public Model Model { get; set; }
        public FileResourceInfo FileResourceInfo { get; set; }
    }
}