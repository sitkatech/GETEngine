using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Microsoft.WindowsAzure.Storage.File;
using Olsson.GET.Common.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;


namespace Olsson.GET.Accessors.FileIO
{
    class BlobFileAccessor : IBlobFileAccessor
    {
        private static readonly ILogger Logger = Logging.GetLogger<BlobFileAccessor>();
        public async Task<byte[]> GetFile(string filePath, string fileLocation)
        {
            var blockBlob = await GetBlockBlobReference(fileLocation, filePath);

            if (blockBlob.ExistsAsync().Result)
            {
                using (var ms = new MemoryStream())
                {
                    await blockBlob.DownloadToStreamAsync(ms);
                    return ms.ToArray();
                }
            }
            return null;
        }

        public async Task GetFile(string filePath, string fileLocation, string destLocation)
        {
            Logger.LogInformation($"Attempting to get file from path: \"{filePath}\", file location: \"{fileLocation}\" and destLocation: \"{destLocation}\"");
            var blockBlob = await GetBlockBlobReference(fileLocation, filePath);

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

            await TransferManager.DownloadAsync(blockBlob, destLocation, downloadOptions, context, CancellationToken.None);
        }

        public async Task<List<string>> GetFilesInDirectory(string directoryPath, string fileLocation)
        {
            var container = await GetCloudBlobContainer(fileLocation);
            var directory = container.GetDirectoryReference(directoryPath);

            BlobContinuationToken blobContinuationToken = null;
            var files = new List<IListBlobItem>();
            do
            {
                var resultSegment = await directory.ListBlobsSegmentedAsync(
                    useFlatBlobListing: true,
                    blobListingDetails: BlobListingDetails.All,
                    maxResults: null,
                    currentToken: blobContinuationToken,
                    options: null,
                    operationContext: null,
                    cancellationToken: CancellationToken.None);

                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = resultSegment.ContinuationToken;
                files.AddRange(resultSegment.Results);
            } while (blobContinuationToken != null);

            return files.Select(a => Uri.UnescapeDataString(a.Uri.Segments.Last())).ToList();
        }

        public async Task SaveFile(string filePath, string fileLocation, byte[] fileContent, string contentType = null)
        {
            var blockBlob = await GetBlockBlobReference(fileLocation, filePath);
            await blockBlob.DeleteIfExistsAsync();

            if (!string.IsNullOrEmpty(contentType))
            {
                blockBlob.Properties.ContentType = contentType;
            }

            using var ms = new MemoryStream(fileContent);
            await blockBlob.UploadFromStreamAsync(ms);
        }

        public async Task SaveFile(string destinationFilePath, string fileLocation, string originFilePath)
        {
            var blockBlob = await GetBlockBlobReference(fileLocation, destinationFilePath);

            // DataMovement will throw an error if file is not deleted
            await blockBlob.DeleteIfExistsAsync();

            // Setup the number of the concurrent operations
            TransferManager.Configurations.ParallelOperations = 64;

            // Setup the transfer context and track the copy progress
            var context = new SingleTransferContext();

            Logger.LogInformation($"saving at destinationFilePath {destinationFilePath}, file location {fileLocation} originfilepath {originFilePath}");
            await TransferManager.UploadAsync(originFilePath, blockBlob, null, context, CancellationToken.None);
        }

        public async Task DeleteFile(string filePath, string fileLocation)
        {
            var blockBlob = await GetBlockBlobReference(fileLocation, filePath);
            await blockBlob.DeleteIfExistsAsync();
        }

        public async Task CreateFileShare(string shareName)
        {
            CloudFileShare cloudFileShare = GetCloudFileShare(shareName);
            await cloudFileShare.CreateIfNotExistsAsync();
        }

        public string GetAgentFileShareSASToken()
        {
            CloudFileShare cloudFileShare = GetCloudFileShare("agent");
             
            var sasToken = cloudFileShare.GetSharedAccessSignature(new SharedAccessFilePolicy()
            {
                Permissions = SharedAccessFilePermissions.Read | SharedAccessFilePermissions.List | SharedAccessFilePermissions.Create | SharedAccessFilePermissions.Write,
                SharedAccessExpiryTime = DateTimeOffset.Now.AddDays(2)
            });
            return sasToken;
        }

        public async Task<List<string>> GetFilesInShareDirectory(string fileLocation, bool recursive = false)
        {
            CloudFileShare cloudFileShare = GetCloudFileShare(fileLocation);

            var directory = cloudFileShare.GetRootDirectoryReference();
            
            FileContinuationToken fileContinuationToken = null;
            var files = new List<IListFileItem>();


            if (recursive)
            {
                do
                { 

                    var resultSegment = await directory.ListFilesAndDirectoriesSegmentedAsync(

                        maxResults: null,
                        currentToken: fileContinuationToken,
                        options: new FileRequestOptions(),
                        operationContext: null,
                        cancellationToken: CancellationToken.None);

                    // Get the value of the continuation token returned by the listing call.
                    fileContinuationToken = resultSegment.ContinuationToken;

                    resultSegment.Results.ToList().ForEach(fileItem =>
                    {
                        if (fileItem is CloudFileDirectory)
                        {
                            var subDirFiles = GetSubDirFiles((CloudFileDirectory)fileItem).Result;
                            files.AddRange(subDirFiles);
                        }
                        else
                        {
                            files.Add(fileItem);
                        }
                    });

                } while (fileContinuationToken != null);
            }
            else
            {
                do
                {
                    var resultSegment = await directory.ListFilesAndDirectoriesSegmentedAsync(

                        maxResults: null,
                        currentToken: fileContinuationToken,
                        options: null,
                        operationContext: null,
                        cancellationToken: CancellationToken.None);

                    // Get the value of the continuation token returned by the listing call.
                    fileContinuationToken = resultSegment.ContinuationToken;

                    files.AddRange(resultSegment.Results);
                } while (fileContinuationToken != null);
            }



            if (recursive)
            {
                return files.Select(a =>
                {
                    var segments = a.Uri.Segments;

                    var message = String.Join("|", segments);
                    Logger.LogInformation($"File share path segments joined with | \"{message}\" found {segments.Length} total segments.");
                    return Uri.UnescapeDataString(String.Join("", segments.Skip(2)));

                }).ToList();
            } 


            return files.Select(a => Uri.UnescapeDataString(a.Uri.Segments.Last())).ToList();
        }


        public async Task<List<IListFileItem>> GetSubDirFiles(CloudFileDirectory directory)
        {
            FileContinuationToken fileContinuationToken = null;
            var files = new List<IListFileItem>();
            do
            {

                var resultSegment = await directory.ListFilesAndDirectoriesSegmentedAsync(

                    maxResults: null,
                    currentToken: fileContinuationToken,
                    options: new FileRequestOptions(),
                    operationContext: null,
                    cancellationToken: CancellationToken.None);

                // Get the value of the continuation token returned by the listing call.
                fileContinuationToken = resultSegment.ContinuationToken;

                resultSegment.Results.ToList().ForEach(fileItem =>
                {
                    if (fileItem is CloudFileDirectory)
                    {
                        var subDirFiles = GetSubDirFiles((CloudFileDirectory)fileItem).Result;
                        files.AddRange(subDirFiles);
                    }
                    else
                    {
                        files.Add(fileItem);
                    }
                });
                
            } while (fileContinuationToken != null);

            return files;
        }

        public async Task GetSharedFile(string srcFilePath, string srcFileLocation, string destLocation)
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

            await TransferManager.DownloadAsync(file, destLocation, downloadOptions, context, CancellationToken.None);
        }

        public async Task CopyFromBlobStorageToFileShare(string srcFilePath, string srcFileLocation, string destFilePath, string destFileLocation, bool deleteSrc = false)
        {
            Logger.LogInformation($"Copying files from blob storage to file share - SRC: [{srcFileLocation}/{srcFilePath}] DEST: [{destFileLocation}/{destFilePath}]");
            var srcblockBlob = await GetBlockBlobReference(srcFileLocation, srcFilePath);

            CloudFileShare cloudFileShare = GetCloudFileShare(destFileLocation);

            var destFile = cloudFileShare.GetRootDirectoryReference().GetFileReference(destFilePath);

            await destFile.StartCopyAsync(srcblockBlob);

            if (deleteSrc)
            {
                await srcblockBlob.DeleteAsync();
            }
        }

        public async Task CopyFromFileShareToBlobStorage(string srcFilePath, string srcFileLocation, string destFilePath, string destFileLocation, bool deleteSrc = false)
        {
            Logger.LogInformation($"Copying files from file share to blob storage - SRC: [{srcFilePath} - {srcFileLocation}] DEST: [{destFilePath} - {destFileLocation}]");
            CloudFileShare cloudFileShare = GetCloudFileShare(srcFileLocation);

            var srcFile = cloudFileShare.GetRootDirectoryReference().GetFileReference(srcFilePath);

            var fsas = srcFile.GetSharedAccessSignature(new SharedAccessFilePolicy()
            {
                Permissions = SharedAccessFilePermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1)
            });
            Uri fileSasUri = new Uri(srcFile.StorageUri.PrimaryUri.ToString() + fsas);


            var destBlockBlob = await GetBlockBlobReference(destFileLocation, destFilePath);

            await destBlockBlob.DeleteIfExistsAsync();

            await destBlockBlob.StartCopyAsync(fileSasUri);

            if (deleteSrc)
            {
                await srcFile.DeleteAsync();
            }
        }

        public async Task DeleteCloudFileShare(string fileLocator)
        {
            var cloudFileShare = GetCloudFileShare(fileLocator);

            await cloudFileShare.DeleteIfExistsAsync();
        }

        #region Private Methods
        private async Task<CloudBlockBlob> GetBlockBlobReference(string containerName, string fileName)
        {
            var container = await GetCloudBlobContainer(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);
            return blockBlob;
        }

        private static async Task<CloudBlobContainer> GetCloudBlobContainer(string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(ConfigurationHelper.ConnectionStrings.AzureStorageAccount);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
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
