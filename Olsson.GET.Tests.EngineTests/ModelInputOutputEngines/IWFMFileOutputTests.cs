using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Engines.ModelInputOutputEngines;

namespace Olsson.GET.Tests.EngineTests.ModelInputOutputEngines
{
    [TestClass]
    public class IWFMFileOutputTests
    {
        [TestMethod]
        public void WorkingDirectoryTest()
        {
            var a = @"C:\Model";
            var b = "Simulation";
            var c = "..\\!bin\\IWFM2015.0.961\\Simulation2015_x64.exe";
            var d = Path.Combine(a, b);
            var e = Path.Combine(d, c);
            var f = Path.GetFullPath(e);
            Assert.AreEqual(@"C:\Model\Simulation", d);
            Assert.AreEqual(@"C:\Model\!bin\IWFM2015.0.961\Simulation2015_x64.exe", f);
        }

    //[TestMethod]
        //public void ProcessBudgetGroundwaterFile()
        //{
        //    var lines = System.IO.File
        //        .ReadLines(System.IO.Path.Combine(@"C:\Model", IWFMModelInputOutputEngine.BudgetGroundwaterFileName))
        //        .SkipWhile(x => !x.Contains("GROUNDWATER BUDGET IN ac.ft. FOR ENTIRE MODEL AREA")).ToList();
        //    var storageAreaLine = lines.Skip(1).Take(1).Single();
        //    var storageArea = double.Parse(storageAreaLine.Substring(storageAreaLine.IndexOf(":") + 2).Replace(" acres", ""));
        //    var a = new IWFMBudgetGroundwaterResult();
        //    a.StorageArea = storageArea;
        //    var result = new List<IWFMBudgetGroundwaterPeriod>();

        //    using (var fileLineEnumerator = lines.Skip(7).GetEnumerator())
        //    {
        //        AddGroundwaterPeriodData(fileLineEnumerator, result);
        //    }

        //    a.Periods = result;
        //    Assert.IsTrue(a.StorageArea > 0);
        //    Assert.IsTrue(a.Periods.Count > 0);
        //}

        //private static void AddGroundwaterPeriodData(IEnumerator<string> fileLineEnumerator, List<IWFMBudgetGroundwaterPeriod> iwfmBudgetGroundwaterPeriods)
        //{
        //    while (fileLineEnumerator.MoveNext())
        //    {
        //        var data = fileLineEnumerator.Current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        //        if (data.Length == 0)
        //        {
        //            continue;
        //        }

        //        iwfmBudgetGroundwaterPeriods.Add(new IWFMBudgetGroundwaterPeriod
        //        {
        //            Time = DateTime.Parse(data[0].Split('_')[0]),
        //            Percolation = double.Parse(data[1]),
        //            BeginningStorage = double.Parse(data[2]),
        //            EndingStorage = double.Parse(data[3]),
        //            DeepPercolation = double.Parse(data[4]),
        //            GainFromStream = double.Parse(data[5]),
        //            GainFromLake = double.Parse(data[7]),
        //            BoundaryInflow = double.Parse(data[8]),
        //            Pumping = double.Parse(data[12]),
        //            OutflowToRootZone = double.Parse(data[13]),
        //        });
        //    }
        //}
    }
}