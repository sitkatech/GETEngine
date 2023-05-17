using System.Runtime.Serialization;

namespace Olsson.GET.Common.DataContracts.FileResource
{
    [DataContract]
    public class FileResourceMimeType
    {
        [DataMember]
        public int FileResourceMimeTypeID { get; set; }

        [DataMember]
        public string FileResourceMimeTypeName { get; set; }

        [DataMember]
        public string FileResourceMimeTypeDisplayName { get; set; }
        
        [DataMember]
        public string FileResourceMimeTypeContentTypeName { get; set; }

        [DataMember]
        public string FileResourceMimeTypeIconSmallFilename { get; set; }

        [DataMember]
        public string FileResourceMimeTypeIconNormalFilename { get; set; }
    }
    public enum FileResourceMimeTypeEnum
    {
        PDF = 1,
        WordDOCX = 2,
        ExcelXLSX = 3,
        XPNG = 4,
        PNG = 5,
        TIFF = 6,
        BMP = 7,
        GIF = 8,
        JPEG = 9,
        PJPEG = 10,
        PowerpointPPTX = 11,
        PowerpointPPT = 12,
        ExcelXLS = 13,
        WordDOC = 14,
        xExcelXLSX = 15,
        CSS = 16,
        XZIP = 17,
        GZIP = 18,
        XGZIP = 19,
        TGZ = 20,
        TAR = 21,
        ZIP = 22,
        KMZ = 23,
        KML = 24
    }

}