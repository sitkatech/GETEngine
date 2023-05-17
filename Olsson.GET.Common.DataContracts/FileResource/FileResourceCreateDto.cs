namespace Olsson.GET.Common.DataContracts.FileResource
{
    public class FileResourceCreateDto
    {
        public FileResourceMimeType FileResourceMimeType { get; set; }
        public string OriginalFilename { get; set; }
        public string OriginalFileExtension { get; set; }
        public int UserID { get; set; }
        public byte[] Data { get; set; }
    }
}