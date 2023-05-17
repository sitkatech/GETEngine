using log4net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Microsoft.WindowsAzure.Storage.File;
using Olsson.GET.Common.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olsson.GET.Accessors.FileIO
{
    class BlobFileAccessor : IBlobFileAccessor
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(BlobFileAccessor));
        public byte[] GetFile(string filePath, string fileLocation)
        {
            var blockBlob = GetBlockBlobReference(fileLocation, filePath);

            if (blockBlob.Exists())
            {
                using (var ms = new MemoryStream())
                {
                    blockBlob.DownloadToStream(ms);
                    return ms.ToArray();
                }
            }
            return null;
        }

        public void GetFile(string filePath, string fileLocation, string destLocation)
        {
            var blockBlob = GetBlockBlobReference(fileLocation, filePath);

            // Setup the number of the concurrent operations
            TransferManager.Configurations.ParallelOperations = 64;

            // Setup the transfer context and track the copy progress
            SingleTransferContext context = new SingleTransferContext();

            //// for debugging: uncomment to see byte progress
            //context.ProgressHandler = new Progress<TransferStatus>((progress) =>
            //{
            //    Console.WriteLine("Bytes Copied: {0}", progress.BytesTransferred);
            //});

            var downloadOptions = new DownloadOptions
            {
                DisableContentMD5Validation = true
            };

            var task = TransferManager.DownloadAsync(blockBlob, destLocation, downloadOptions, context, CancellationToken.None);
            task.Wait();
        }

        public List<string> GetFilesInDirectory(string directoryPath, string fileLocation)
        {
            var container = GetCloudBlobContainer(fileLocation);
            var directory = container.GetDirectoryReference(directoryPath);
            return directory.ListBlobs().OfType<CloudBlockBlob>().Select(a => Uri.UnescapeDataString(a.Uri.Segments.Last())).ToList();
        }

        public void SaveFile(string filePath, string fileLocation, byte[] fileContent, string contentType = null)
        {
            var blockBlob = GetBlockBlobReference(fileLocation, filePath);

            if (blockBlob.Exists())
                blockBlob.DeleteIfExists();

            if (!string.IsNullOrEmpty(contentType))
            {
                blockBlob.Properties.ContentType = contentType;
            }

            using (var ms = new MemoryStream(fileContent))
            {
                blockBlob.UploadFromStream(ms);
            }
        }

        public void SaveFile(string destinationFilePath, string fileLocation, string originFilePath)
        {
            var blockBlob = GetBlockBlobReference(fileLocation, destinationFilePath);

            // DataMovement will throw an error if file is not deleted
            blockBlob.DeleteIfExists();

            // Setup the number of the concurrent operations
            TransferManager.Configurations.ParallelOperations = 64;

            // Setup the transfer context and track the copy progress
            var context = new SingleTransferContext();

            var task = TransferManager.UploadAsync(originFilePath, blockBlob, null, context, CancellationToken.None);
            task.Wait();
        }

        public void DeleteFile(string filePath, string fileLocation)
        {
            var blockBlob = GetBlockBlobReference(fileLocation, filePath);

            if (blockBlob.Exists())
            {
                blockBlob.DeleteIfExists();
            }
        }

        public void CreateFileShare(string shareName)
        {
            CloudFileShare cloudFileShare = GetCloudFileShare(shareName);
            cloudFileShare.CreateIfNotExists();
        }

        public List<string> GetFilesInShareDirectory(string fileLocation)
        {
            CloudFileShare cloudFileShare = GetCloudFileShare(fileLocation);

            var directory = cloudFileShare.GetRootDirectoryReference();
            return directory.ListFilesAndDirectories().Select(a => Uri.UnescapeDataString(a.Uri.Segments.Last())).ToList();
        }

        public void GetSharedFile(string srcFilePath, string srcFileLocation, string destLocation)
        {
            CloudFileShare cloudFileShare = GetCloudFileShare(srcFileLocation);

            var file = cloudFileShare.GetRootDirectoryReference().GetFileReference(srcFilePath);

            // Setup the number of the concurrent operations
            TransferManager.Configurations.ParallelOperations = 64;

            // Setup the transfer context and track the copy progress
            var context = new SingleTransferContext();
            var downloadOptions = new DownloadOptions
            {
                DisableContentMD5Validation = true
            };

            var task = TransferManager.DownloadAsync(file, destLocation, downloadOptions, context, CancellationToken.None);
            task.Wait();
        }

        public void CopyFromBlobStorageToFileShare(string srcFilePath, string srcFileLocation, string destFilePath, string destFileLocation, bool deleteSrc = false)
        {
            Logger.Info($"Copying files from blob storage to file share - SRC: [{srcFileLocation}/{srcFilePath}] DEST: [{destFileLocation}/{destFilePath}]");
            var srcblockBlob = GetBlockBlobReference(srcFileLocation, srcFilePath);

            CloudFileShare cloudFileShare = GetCloudFileShare(destFileLocation);

            var destFile = cloudFileShare.GetRootDirectoryReference().GetFileReference(destFilePath);

            destFile.StartCopy(srcblockBlob);

            if (deleteSrc)
            {
                srcblockBlob.Delete();
            }
        }

        public void CopyFromFileShareToBlobStorage(string srcFilePath, string srcFileLocation, string destFilePath, string destFileLocation, bool deleteSrc = false)
        {
            Logger.Info($"Copying files from file share to blob storage - SRC: [{srcFilePath} - {srcFileLocation}] DEST: [{destFilePath} - {destFileLocation}]");
            CloudFileShare cloudFileShare = GetCloudFileShare(srcFileLocation);

            var srcFile = cloudFileShare.GetRootDirectoryReference().GetFileReference(srcFilePath);

            var fsas = srcFile.GetSharedAccessSignature(new SharedAccessFilePolicy()
            {
                Permissions = SharedAccessFilePermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1)
            });
            Uri fileSasUri = new Uri(srcFile.StorageUri.PrimaryUri.ToString() + fsas);


            var destBlockBlob = GetBlockBlobReference(destFileLocation, destFilePath);

            destBlockBlob.DeleteIfExists();

            destBlockBlob.StartCopy(fileSasUri);

            if (deleteSrc)
            {
                srcFile.Delete();
            }
        }

        public void DeleteCloudFileShare(string fileLocator)
        {
            var cloudFileShare = GetCloudFileShare(fileLocator);

            if (cloudFileShare.Exists())
            {
                cloudFileShare.Delete();
            }
        }

        #region Private Methods
        private CloudBlockBlob GetBlockBlobReference(string containerName, string fileName)
        {
            var container = GetCloudBlobContainer(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);
            return blockBlob;
        }

        private static CloudBlobContainer GetCloudBlobContainer(string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(ConfigurationHelper.ConnectionStrings.AzureStorageAccount);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();
            return container;
        }

        private static CloudFileShare GetCloudFileShare(string fileLocator)
        {
            return CloudStorageAccount.Parse(
                 ConfigurationHelper.ConnectionStrings.AzureStorageAccount)
                 .CreateCloudFileClient()
                 .GetShareReference(fileLocator);
        }

        #endregion
    }
}
