using System.Collections.Generic;

namespace Olsson.GET.Accessors.FileIO
{
    public interface IBlobFileAccessor
    {
        void SaveFile(string filePath, string fileLocation, byte[] fileContent, string contentType = null);

        byte[] GetFile(string filePath, string fileLocation);

        void GetFile(string filePath, string fileLocation, string destLocation);

        List<string> GetFilesInDirectory(string directoryPath, string fileLocation);

        void SaveFile(string destinationFilePath, string fileLocation, string originFilePath);

        void DeleteFile(string filePath, string fileLocation);

        void CreateFileShare(string shareName);

        List<string> GetFilesInShareDirectory(string fileLocation);

        void GetSharedFile(string srcFilePath, string srcFileLocation, string destLocation);

        void CopyFromBlobStorageToFileShare(string srcFilePath, string srcFileLocation, string destFilePath, string destFileLocation, bool deleteSrc = false);

        void CopyFromFileShareToBlobStorage(string srcFilePath, string srcFileLocation, string destFilePath, string destFileLocation, bool deleteSrc = false);

        void DeleteCloudFileShare(string fileLocator);
    }
}
