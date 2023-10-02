using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors;
using Olsson.GET.Accessors.Containers;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.Utilities;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;

namespace Olsson.GET.Tests.AccessorTests;

[TestClass]
public class BlobStorageAccessorTests : BaseAccessorTest
{
    private readonly IContainerAccessor _containerAccessor = new AccessorFactory().CreateAccessor<IContainerAccessor>();
    private readonly IBlobFileAccessor _blobFileAccessor = new AccessorFactory().CreateAccessor<IBlobFileAccessor>();
    private readonly IFileAccessor _fileAccessor = new AccessorFactory().CreateAccessor<IFileAccessor>();

    [TestMethod]
    public void CanUploadDirectoryToFileStorage()
    {

        ConfigurationHelper.AppSettings.ModflowDataFolder = $@"IWFMTestFiles";
        // get files in the local directory
        var modelFiles = _fileAccessor.GetFilesInModflowDataFolder(true);
        var storageFilesCopied = new List<string>();

        // create the file share to test with
        var fileStorageLocator = "accessor-tests";
        var storageFiles = _blobFileAccessor.GetFilesInShareDirectory(fileStorageLocator, true).Result;

        // replace files in local model directory if they are found in the file share
        foreach (var storageFile in storageFiles)
        {
            var destinationPath = $"{ConfigurationHelper.AppSettings.ModflowDataFolder}\\{storageFile.Replace("/", "\\")}";

            if (modelFiles.Any(x => x.Path.Equals(destinationPath, StringComparison.InvariantCultureIgnoreCase)))
            {
                _fileAccessor.DeleteFile(destinationPath);
                _blobFileAccessor.GetSharedFile(storageFile, fileStorageLocator, destinationPath).Wait();
                storageFilesCopied.Add(destinationPath);
            }
        }

        Assert.IsNotNull(modelFiles);
        Assert.IsNotNull(storageFilesCopied);
    }


}