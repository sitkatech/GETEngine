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
using Olsson.GET.Common.Shared;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.DataContracts.APIFunctionModels;
using Olsson.GET.Common.DataContracts.Runs;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using ModelExecutable = Olsson.GET.Common.DataContracts.Models.ModelExecutable;
using CsvHelper;
using NetTopologySuite.Geometries;
using static Olsson.GET.Accessors.FileIO.ModelFileAccessor;
using System.Globalization;
using System.Reflection.PortableExecutable;

namespace Olsson.GET.Tests.AccessorTests;

[TestClass]
public class BlobStorageAccessorTests : BaseAccessorTest
{
    private readonly IContainerAccessor _containerAccessor = new AccessorFactory().CreateAccessor<IContainerAccessor>();
    private readonly IBlobFileAccessor _blobFileAccessor = new AccessorFactory().CreateAccessor<IBlobFileAccessor>();
    private readonly IFileAccessor _fileAccessor = new AccessorFactory().CreateAccessor<IFileAccessor>();
    //private readonly IModelFileAccessor _modelFileAccessor = new ModelFileAccessorFactory().CreateModflowFileAccessor(new Model
    //{
    //    ModelEngineTypeID = (int)ModelEngineTypeEnum.IWFM,
    //});

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
                storageFilesCopied.Add(storageFile);
            }
        }


        // test uploading to blob storage to see if folders work there
        foreach (var file in storageFilesCopied)
        {
            _blobFileAccessor.CopyFromFileShareToBlobStorage(file, fileStorageLocator,
                StorageLocations.ModelOutputFolderPath(fileStorageLocator, file),
                ConfigurationHelper.AppSettings.BlobStorageModelOutputsFolder).Wait();
        }


        Assert.IsNotNull(modelFiles);
        Assert.IsNotNull(storageFilesCopied);
    }

    [TestMethod]
    public void FileShareDirectoryTest()
    {
        var storageFiles = _blobFileAccessor.GetFilesInShareDirectory("3bcca1a5-7bca-4fc2-a5e5-58af4556c330", true).Result;
        Assert.IsNotNull(storageFiles);
    }

    [TestMethod]
    public void GetUserDataFileTest()
    {
        ConfigurationHelper.AppSettings.ModflowDataFolder = $@"IWFMTestFiles";
        var testFileStorageLocator = "1f06e793-827a-4aa3-a236-fd8940b6d395";
        var file = _blobFileAccessor.GetFile(StorageLocations.UserDataFilePathForRun(testFileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
        var userDataObject = JsonConvert.DeserializeObject<UserDataJson>(Encoding.UTF8.GetString(file));
        
        var modelFileAccessor = new ModelFileAccessorFactory().CreateModflowFileAccessor(new Model
        {
            ModelEngineTypeID = (int)ModelEngineTypeEnum.IWFM,
        });

        var nodeLocations = modelFileAccessor.GetIWFMNodeLocations();

        Dictionary<int, Point> nodePoints = nodeLocations.ToDictionary(x => x.Key, x => new Point(x.Value.Item2, x.Value.Item1));
        // find the closest node to each of the input locations
        userDataObject.UserDataPointInputs.ForEach(inputPoint =>
        {
            var inputPointGeometry = new Point(inputPoint.Lng, inputPoint.Lat);
            var closestNode = nodePoints.Keys.Select(x => new
                { Node = x, Distance = nodePoints[x].Distance(inputPointGeometry) })
                .OrderBy(x => x.Distance)
                .First().Node;

            inputPoint.ClosestNode = closestNode;
        });

        // since we have the closest node, now we need to parse the HeadAll.out file for the model and get the results for the node.

        var nodeValuesDictionary = new Dictionary<DateTime, Dictionary<int,List<double>>>();
        var reader = modelFileAccessor.GetIWFMHeadAllOutputFile();
        string line;
        DateTime currentTimeStep = default;
        while ((line = reader.ReadLine()) != null)
        {
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("*"))
                continue;

            string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            var isNewTimestepRow = DateTime.TryParseExact(parts[0].Replace("24:00", "00:00"), "MM/dd/yyyy_HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime dateTime);

            if (isNewTimestepRow)
            {
                currentTimeStep = dateTime; // may need to add a day here due to the 24:00 -> 00:00 above
                parts = parts.Skip(1).ToArray();
                // the rest of the parts should now be just the number of nodes
            }

            for (int i = 0; i < parts.Length; i++)
            {
                if(!nodeValuesDictionary.ContainsKey(currentTimeStep))
                {
                    nodeValuesDictionary.Add(currentTimeStep, new Dictionary<int, List<double>>());
                }

                if (!nodeValuesDictionary[currentTimeStep].ContainsKey(i + 1))
                {
                    nodeValuesDictionary[currentTimeStep][i + 1] = new List<double>();
                }

                nodeValuesDictionary[currentTimeStep][i + 1].Add(Double.Parse(parts[i]));
            }
        }


        userDataObject.UserDataPointInputs.ForEach(inputPoint =>
        {
            inputPoint.TimeSteps = new List<UserDataPointTimeStep>();

            foreach (var dateTime in nodeValuesDictionary.Keys)
            {
                var valueToAdd = nodeValuesDictionary[dateTime][inputPoint.ClosestNode].Last();
                inputPoint.TimeSteps.Add(new UserDataPointTimeStep() { DateTime = dateTime, Value = valueToAdd });
            }
        });
        _blobFileAccessor
            .SaveFile(
                StorageLocations.OutputFilePathForRun(testFileStorageLocator,
                    $"{2.ToString().PadLeft(3, '0')}-TimeSeriesData.json"),
                ConfigurationHelper.AppSettings.BlobStorageModelDataFolder,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userDataObject))).Wait();

        Assert.IsNotNull(userDataObject);
    }


}