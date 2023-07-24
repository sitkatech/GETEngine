using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olsson.GET.Accessors.FileIO
{
    public interface IBlobFileAccessor
    {
        Task SaveFile(string filePath, string fileLocation, byte[] fileContent, string contentType = null);

        Task<byte[]> GetFile(string filePath, string fileLocation);

        Task GetFile(string filePath, string fileLocation, string destLocation);

        Task<List<string>> GetFilesInDirectory(string directoryPath, string fileLocation);

        Task SaveFile(string destinationFilePath, string fileLocation, string originFilePath);

        Task DeleteFile(string filePath, string fileLocation);

        Task CreateFileShare(string shareName);

        Task<List<string>> GetFilesInShareDirectory(string fileLocation);

        Task GetSharedFile(string srcFilePath, string srcFileLocation, string destLocation);

        Task CopyFromBlobStorageToFileShare(string srcFilePath, string srcFileLocation, string destFilePath, string destFileLocation, bool deleteSrc = false);

        Task CopyFromFileShareToBlobStorage(string srcFilePath, string srcFileLocation, string destFilePath, string destFileLocation, bool deleteSrc = false);

        Task DeleteCloudFileShare(string fileLocator);
    }
}
