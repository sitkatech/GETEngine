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

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    public class IWFMModelInputOutputEngine : BaseInputOutputEngine, IModelInputOutputEngine
    {
        public const string BudgetGroundwaterFileName = "Budget/Groundwater.bud";

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

            var lines = System.IO.File.ReadLines(System.IO.Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, BudgetGroundwaterFileName)).SkipWhile(x => x.IndexOf("groundwater budget in ac.ft. for entire model area", StringComparison.InvariantCultureIgnoreCase) < 0).ToList();

            var storageAreaLine = lines.Skip(1).Take(1).Single();
            var storageArea = double.Parse(storageAreaLine.Substring(storageAreaLine.IndexOf(":", StringComparison.InvariantCultureIgnoreCase) + 2).Split(' ')[0]);
            var iwfmBudgetGroundwaterResult = new IWFMBudgetGroundwaterResult
            {
                StorageArea = storageArea
            };
            var iwfmBudgetGroundwaterPeriods = new List<IWFMBudgetGroundwaterPeriod>();

            using (var fileLineEnumerator = lines.Skip(7).GetEnumerator())
            {
                AddGroundwaterPeriodData(fileLineEnumerator, iwfmBudgetGroundwaterPeriods);
            }
            iwfmBudgetGroundwaterResult.Periods = iwfmBudgetGroundwaterPeriods;

            fileAccessor.SaveFile(StorageLocations.OutputFilePathForRun(run.FileStorageLocator, $"{currResultId.ToString().PadLeft(3, '0')}-GroundWaterBudget.json"), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(iwfmBudgetGroundwaterResult)));
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
                    GainFromLake = double.Parse(data[7]),
                    BoundaryInflow = double.Parse(data[8]),
                    Pumping = double.Parse(data[12]),
                    OutflowToRootZone = double.Parse(data[13]),
                });
            }
        }

        private static string[] GetColumnLineData(string line, char separator = ' ')
        {
            return line.Trim().Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
        }

    }
}