using System;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;
using System.Collections.Generic;
using System.Linq;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using Newtonsoft.Json;
using Olsson.GET.Common.Shared;
using System.Text;
using NetTopologySuite.Geometries;
using Olsson.GET.Accessors.EntityFramework;
using System.IO;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;
using System.Globalization;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    public class IWFMModelInputOutputEngine : BaseInputOutputEngine, IModelInputOutputEngine
    {
        public const string BudgetGroundwaterFileName = "Budget/Groundwater.bud";
        public const string BaselineBudgetGroundwaterFileName = "Budget/Groundwater.Baseline.bud";

        private Model Model { get; }

        public IWFMModelInputOutputEngine(Model model)
        {
            Model = model;
            AccessorFactory = new Accessors.AccessorFactory();
        }

        public void GenerateInputFiles(Run run)
        {
            // TODO: intentionally doing nothing right now for MVP; all we need to do is create the file share if it doesn't exist
        }

        public void GenerateOutputFiles(Run run)
        {
            var currResultId = 1;

            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
            var modelFileAccessor = AccessorFactory.CreateAccessor<IModelFileAccessorFactory>().CreateModflowFileAccessor(Model);

            CreateGroundwaterBudget(run, fileAccessor, currResultId);
            currResultId++;
            CreateGroundwaterLevelPoints(run, fileAccessor, modelFileAccessor, currResultId);

        }

        private void CreateGroundwaterLevelPoints(Run run, IBlobFileAccessor fileAccessor,
            IModelFileAccessor modelFileAccessor, int currResultId)
        {
            // get the userdata.json file (this might not be relevant for all IWFM runs but we need it now)
            // the points in here will have all of the points we want to get information from
            var file = fileAccessor.GetFile(StorageLocations.UserDataFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            var userDataObject = JsonConvert.DeserializeObject<UserDataJson>(Encoding.UTF8.GetString(file));


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

            var parsedHeadAllOutputFile = ParseHeadAllOutputFile(modelFileAccessor);
            var baselineHeadAllOutputFile = ParseHeadAllOutputFile(modelFileAccessor, true);

            userDataObject.UserDataPointInputs.ForEach(inputPoint =>
            {
                inputPoint.TimeSteps = new List<UserDataPointTimeStep>();

                foreach (var dateTime in parsedHeadAllOutputFile.Keys)
                {
                    var runValue = parsedHeadAllOutputFile[dateTime][inputPoint.ClosestNode].Last();
                    double? baselineValue = null;
                    double? baselineValueDifference = null;
                    if (run.IsDifferential)
                    {
                        // get the difference between baseline and the run value for differential results
                        baselineValue = baselineHeadAllOutputFile[dateTime][inputPoint.ClosestNode].Last();
                        baselineValueDifference = baselineValue - runValue;
                    }
                    else
                    {
                        runValue = parsedHeadAllOutputFile[dateTime][inputPoint.ClosestNode].Last();
                    }

                    inputPoint.TimeSteps.Add(new UserDataPointTimeStep()
                    {
                        DateTime = dateTime,
                        Value = runValue,
                        BaselineValue = baselineValue,
                        BaselineValueDifference = baselineValueDifference
                    });
                }
            });

            fileAccessor
                .SaveFile(
                    StorageLocations.OutputFilePathForRun(run.FileStorageLocator,
                        $"{currResultId.ToString().PadLeft(3, '0')}-TimeSeriesData.json"),
                    ConfigurationHelper.AppSettings.BlobStorageModelDataFolder,
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userDataObject))).Wait();

        }

        private static void CreateGroundwaterBudget(Run run, IBlobFileAccessor fileAccessor, int currResultId)
        {
            var lines = System.IO.File
                .ReadLines(System.IO.Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, BudgetGroundwaterFileName))
                .SkipWhile(x => x.IndexOf("groundwater budget in ac.ft. for entire model area",
                    StringComparison.InvariantCultureIgnoreCase) < 0).ToList();

            var storageAreaLine = lines.Skip(1).Take(1).Single();
            var storageArea = double.Parse(storageAreaLine
                .Substring(storageAreaLine.IndexOf(":", StringComparison.InvariantCultureIgnoreCase) + 2).Split(' ')[0]);
            var iwfmBudgetGroundwaterResult = new IWFMBudgetGroundwaterResult
            {
                StorageArea = storageArea
            };

            // run result periods
            var iwfmBudgetGroundwaterPeriods = new List<IWFMBudgetGroundwaterPeriod>();
            using (var fileLineEnumerator = lines.Skip(7).GetEnumerator())
            {
                AddGroundwaterPeriodData(fileLineEnumerator, iwfmBudgetGroundwaterPeriods);
            }
            iwfmBudgetGroundwaterResult.Periods = iwfmBudgetGroundwaterPeriods;

            // baseline periods
            if (run.IsDifferential)
            {
                var baselineLines
                    = System.IO.File
                    .ReadLines(System.IO.Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, BaselineBudgetGroundwaterFileName))
                    .SkipWhile(x => x.IndexOf("groundwater budget in ac.ft. for entire model area",
                        StringComparison.InvariantCultureIgnoreCase) < 0).ToList();

                var baselineStorageAreaLine = baselineLines.Skip(1).Take(1).Single();
                var iwfmBaselineBudgetGroundwaterPeriods = new List<IWFMBudgetGroundwaterPeriod>();
                using (var fileLineEnumerator = baselineLines.Skip(7).GetEnumerator())
                {
                    AddGroundwaterPeriodData(fileLineEnumerator, iwfmBaselineBudgetGroundwaterPeriods);
                }
                iwfmBudgetGroundwaterResult.BaselinePeriods = iwfmBaselineBudgetGroundwaterPeriods;
            }

            fileAccessor
                .SaveFile(
                    StorageLocations.OutputFilePathForRun(run.FileStorageLocator,
                        $"{currResultId.ToString().PadLeft(3, '0')}-GroundWaterBudget.json"),
                    ConfigurationHelper.AppSettings.BlobStorageModelDataFolder,
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(iwfmBudgetGroundwaterResult))).Wait();
        }


        private static void AddGroundwaterPeriodData(IEnumerator<string> fileLineEnumerator, List<IWFMBudgetGroundwaterPeriod> iwfmBudgetGroundwaterPeriods)
        {
            while (fileLineEnumerator.MoveNext())
            {
                var data = GetColumnLineData(fileLineEnumerator.Current);

                if (data.Length == 0)
                {
                    continue;
                }

                iwfmBudgetGroundwaterPeriods.Add(new IWFMBudgetGroundwaterPeriod
                {
                    Time = DateTime.Parse(data[0].Split('_')[0]),
                    Percolation = double.Parse(data[1]),
                    BeginningStorage = double.Parse(data[2]),
                    EndingStorage = double.Parse(data[3]),
                    DeepPercolation = double.Parse(data[4]),
                    GainFromStream = double.Parse(data[5]),
                    Recharge = double.Parse(data[6]),
                    GainFromLake = double.Parse(data[7]),
                    BoundaryInflow = double.Parse(data[8]),
                    Subsidence = double.Parse(data[9]),
                    SubsurfaceIrrigation = double.Parse(data[10]),
                    TileDrainOutflow = double.Parse(data[11]),
                    Pumping = double.Parse(data[12]),
                    OutflowToRootZone = double.Parse(data[13]),
                    NetSubsurfaceInflow = double.Parse(data[14]),
                    Discrepancy = double.Parse(data[15]),
                    CumulativeSubsidence = double.Parse(data[16]),
                });
            }
        }

        private static string[] GetColumnLineData(string line, char separator = ' ')
        {
            return line.Trim().Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
        }

        private Dictionary<DateTime, Dictionary<int, List<double>>> ParseHeadAllOutputFile(
            IModelFileAccessor modelFileAccessor, bool isDifferential = false)
        {
            var nodeValuesDictionary = new Dictionary<DateTime, Dictionary<int, List<double>>>();
            var reader = modelFileAccessor.GetIWFMHeadAllOutputFile(isDifferential);
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
                    if (!nodeValuesDictionary.ContainsKey(currentTimeStep))
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
            return nodeValuesDictionary;
        }

        
    }
}