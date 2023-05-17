using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Engines.ModelInputOutputEngines;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using ModelStressPeriodCustomStartDate = Olsson.GET.Common.DataContracts.Models.ModelStressPeriodCustomStartDate;

namespace Olsson.GET.Tests.EngineTests.ModelInputOutputEngines
{
    [TestClass]
    public class ListFileOutputSubEngineTests
    {
        private readonly Model _model = new Model
        {
            StartDateTime = new DateTime(2011, 1, 1),
            AllowablePercentDiscrepancy = 1.0
        };

        private List<StressPeriod> _getStressPeriodDataResult = new List<StressPeriod>
        {
            new StressPeriod
            {
                Days = 31,
                NumberOfTimeSteps = 1
            },
            new StressPeriod
            {
                Days = 28,
                NumberOfTimeSteps = 1
            }
        };

        private List<StressPeriod> _getStressPeriodDataAnnualResult = new List<StressPeriod>
        {
            new StressPeriod
            {
                Days = 31,
                NumberOfTimeSteps = 1
            },
            new StressPeriod
            {
                Days = 31,
                NumberOfTimeSteps = 1
            }
        };

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Total_TooHigh_EndOfFile()
        {
            PercentDiscrepancyOutOfRange("0.00", "0.00", "1.01", "0.00");
        }

        [TestMethod]
        public void GenerateListFileOutputs_PercentDiscrepancy_Total_TooHigh_StartOfFile()
        {
            PercentDiscrepancyOutOfRange("1.01", "0.00", "0.00", "0.00");
        }

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Step_TooHigh_StartOfFile()
        {
            PercentDiscrepancyOutOfRange("0.00", "1.01", "0.00", "0.00");
        }

        [TestMethod]
        public void GenerateListFileOutputs_PercentDiscrepancy_Step_TooHigh_EndOfFile()
        {
            PercentDiscrepancyOutOfRange("0.00", "0.00", "0.00", "1.01");
        }

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Total_TooLow_EndOfFile()
        {
            PercentDiscrepancyOutOfRange("0.00", "0.00", "-1.01", "0.00");
        }

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Total_TooLow_StartOfFile()
        {
            PercentDiscrepancyOutOfRange("-1.01", "0.00", "0.00", "0.00");
        }

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Step_TooLow_StartOfFile()
        {
            PercentDiscrepancyOutOfRange("0.00", "-1.01", "0.00", "0.00");
        }

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Step_TooLow_EndOfFile()
        {
            PercentDiscrepancyOutOfRange("0.00", "0.00", "0.00", "-1.01");
        }

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Total_HighEnd_EndOfFile()
        {
            PercentDiscrepancyInRange("0.00", "0.00", "1.00", "0.00");
        }

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Total_HighEnd_StartOfFile()
        {
            PercentDiscrepancyInRange("1.00", "0.00", "0.00", "0.00");
        }

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Step_HighEnd_StartOfFile()
        {
            PercentDiscrepancyInRange("0.00", "1.00", "0.00", "0.00");
        }

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Step_HighEnd_EndOfFile()
        {
            PercentDiscrepancyInRange("0.00", "0.00", "0.00", "1.00");
        }

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Total_LowEnd_EndOfFile()
        {
            PercentDiscrepancyInRange("0.00", "0.00", "-1.00", "0.00");
        }

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Total_LowEnd_StartOfFile()
        {
            PercentDiscrepancyInRange("-1.00", "0.00", "0.00", "0.00");
        }

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Step_LowEnd_StartOfFile()
        {
            PercentDiscrepancyInRange("0.00", "-1.00", "0.00", "0.00");
        }

        [TestMethod]
        public void GenerateListFileOutput_PercentDiscrepancy_Step_LowEnd_EndOfFile()
        {
            PercentDiscrepancyInRange("0.00", "0.00", "0.00", "-1.00");
        }

        [TestMethod]
        public void GenerateListFileOutput_NonDifferential()
        {
            var totalFirst = "0.00";
            var stepFirst = "0.00";
            var totalLast = "1.01";
            var stepLast = "0.00";

            var listFileData = new List<string>
            {
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   1",
                $" AAA =           {totalFirst}     AAA =          {stepFirst}",
                $" TOTAL IN =           {0}     TOTAL IN =           {0}",
                $" PERCENT DISCREPANCY =           {totalFirst}     PERCENT DISCREPANCY =          {stepFirst}",
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   1",
                $" AAA =           {totalLast}     AAA =           {stepLast}",
                $" TOTAL IN =           {0}     TOTAL IN =           {0}",
                $" PERCENT DISCREPANCY =           {totalLast}     PERCENT DISCREPANCY =           {stepLast}",
            };
            _modflowFileAccessorMock.Arrange(a => a.GetRunListFileLines())
                .Returns(listFileData);
            _modflowFileAccessorMock.Arrange(a => a.GetListFileOutputFileLines())
                .Returns(listFileData);

            _modflowFileAccessorMock.Arrange(a => a.GetAsrDataNameMap())
                .Returns(new List<AsrDataMap>
                {
                    new AsrDataMap { Key = "AAA", Name = "AAA" }
                });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedZoneBudget(false)).Returns((IEnumerable<ObservedZoneBudgetData>)null);

            var sut = CreateListFileOutputSubEngine();
            var result = sut.GenerateListFileOutput(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, false);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOfType(result.Exception, typeof(OutputDataInvalidException));
            Assert.IsNotNull(result.OutputResults);
            Assert.AreEqual(2, result.OutputResults.Count);
            result.OutputResults[0].RunResultName.Should().Be("Water Budget");
            result.OutputResults[0].ResultSets.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].Name.Should().Be("Rate");
            result.OutputResults[0].ResultSets[0].DataSeries.Count.Should().Be(1);
            result.OutputResults[0].ResultSets[0].DataSeries[0].Name.Should().Be("AAA");
            result.OutputResults[0].ResultSets[0].DataSeries[0].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[1].Name.Should().Be("Cumulative");
            result.OutputResults[0].ResultSets[1].DataSeries.Count.Should().Be(1);
            result.OutputResults[0].ResultSets[1].DataSeries[0].Name.Should().Be("AAA");
            result.OutputResults[0].ResultSets[1].DataSeries[0].DataPoints.Count.Should().Be(2);

            TestListFileResult(result.OutputResults[1], listFileData);
        }

        [TestMethod]
        public void GenerateListFileOutput_NonDifferentialAnnual()
        {
            var totalFirst = "0.00";
            var stepFirst = "0.00";
            var totalLast = "1.01";
            var stepLast = "0.00";

            var listFileData = new List<string>
            {
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   1",
                $" AAA =           {totalFirst}     AAA =          {stepFirst}",
                $" TOTAL IN =           {0}     TOTAL IN =           {0}",
                $" PERCENT DISCREPANCY =           {totalFirst}     PERCENT DISCREPANCY =          {stepFirst}",
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   1",
                $" AAA =           {totalLast}     AAA =           {stepLast}",
                $" TOTAL IN =           {0}     TOTAL IN =           {0}",
                $" PERCENT DISCREPANCY =           {totalLast}     PERCENT DISCREPANCY =           {stepLast}",
            };
            _modflowFileAccessorMock.Arrange(a => a.GetRunListFileLines())
                .Returns(listFileData);
            _modflowFileAccessorMock.Arrange(a => a.GetListFileOutputFileLines())
                .Returns(listFileData);

            _modflowFileAccessorMock.Arrange(a => a.GetAsrDataNameMap())
                .Returns(new List<AsrDataMap>
                {
                    new AsrDataMap { Key = "AAA", Name = "AAA" }
                });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedZoneBudget(false)).Returns((IEnumerable<ObservedZoneBudgetData>)null);

            var sut = CreateListFileOutputSubEngineAnnual();
            var result = sut.GenerateListFileOutput(_modflowFileAccessorMock, _getStressPeriodDataAnnualResult, VolumeUnit.AcreFeet.VolumeUnitID, false);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOfType(result.Exception, typeof(OutputDataInvalidException));
            Assert.IsNotNull(result.OutputResults);
            Assert.AreEqual(2, result.OutputResults.Count);
            result.OutputResults[0].RunResultName.Should().Be("Water Budget");
            result.OutputResults[0].ResultSets.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].Name.Should().Be("Rate");
            result.OutputResults[0].ResultSets[0].DataSeries.Count.Should().Be(1);
            result.OutputResults[0].ResultSets[0].DataSeries[0].Name.Should().Be("AAA");
            result.OutputResults[0].ResultSets[0].DataSeries[0].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].DataSeries[0].DataPoints[1].Date.Should()
                .Be(new DateTime(2012, 1, 1));
            result.OutputResults[0].ResultSets[1].Name.Should().Be("Cumulative");
            result.OutputResults[0].ResultSets[1].DataSeries.Count.Should().Be(1);
            result.OutputResults[0].ResultSets[1].DataSeries[0].Name.Should().Be("AAA");
            result.OutputResults[0].ResultSets[1].DataSeries[0].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].DataSeries[0].DataPoints[1].Date.Should()
                .Be(new DateTime(2012, 1, 1));

            TestListFileResult(result.OutputResults[1], listFileData);
        }

        [TestMethod]
        public void GenerateListFileOutput_NonDifferential_RepeatingPackages()
        {
            var totalInFirst = "0.00";
            var totalInSecond = "1.00";
            var stepInFirst = "0.00";
            var stepInSecond = "2.00";
            var totalOutFirst = "0.00";
            var totalOutSecond = "1.00";
            var stepOutFirst = "0.00";
            var stepOutSecond = "2.00";
            var totalLast = "1.01";
            var stepLast = "0.00";

            var listFileData = new List<string>
            {
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   1",
                $" WEL =           {totalInFirst}     WEL =          {stepInFirst}",
                $" WEL =           {totalInSecond}     WEL =          {stepInSecond}",
                $" TOTAL IN =           {0}     TOTAL IN =           {0}",
                $" WEL =           {totalOutFirst}     WEL =          {stepOutFirst}",
                $" WEL =           {totalOutSecond}     WEL =          {stepOutSecond}",
                $" PERCENT DISCREPANCY =           {totalInFirst}     PERCENT DISCREPANCY =          {stepInFirst}",
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   2",
                $" WEL =           {totalInFirst}     WEL =          {stepInFirst}",
                $" WEL =           {totalInSecond}     WEL =          {stepInSecond}",
                $" TOTAL IN =           {0}     TOTAL IN =           {0}",
                $" WEL =           {totalOutFirst}     WEL =          {stepOutFirst}",
                $" WEL =           {totalOutSecond}     WEL =          {stepOutSecond}",
                $" PERCENT DISCREPANCY =           {totalLast}     PERCENT DISCREPANCY =           {stepLast}",
            };
            _modflowFileAccessorMock.Arrange(a => a.GetRunListFileLines())
                .Returns(listFileData);
            _modflowFileAccessorMock.Arrange(a => a.GetListFileOutputFileLines())
                .Returns(listFileData);

            _modflowFileAccessorMock.Arrange(a => a.GetAsrDataNameMap())
                .Returns(new List<AsrDataMap>
                {
                    new AsrDataMap { Key = "WEL", Name = "WEL" }
                });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedZoneBudget(false)).Returns((IEnumerable<ObservedZoneBudgetData>)null);

            var sut = CreateListFileOutputSubEngine();
            var result = sut.GenerateListFileOutput(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, false);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOfType(result.Exception, typeof(OutputDataInvalidException));
            Assert.IsNotNull(result.OutputResults);
            Assert.AreEqual(2, result.OutputResults.Count);
            result.OutputResults[0].RunResultName.Should().Be("Water Budget");
            result.OutputResults[0].ResultSets.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].Name.Should().Be("Rate");
            result.OutputResults[0].ResultSets[0].DataSeries.Count.Should().Be(1);
            result.OutputResults[0].ResultSets[0].DataSeries[0].Name.Should().Be("WEL");
            result.OutputResults[0].ResultSets[0].DataSeries[0].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].DataSeries[0].DataPoints[0].Value.Should().Be(0);
            result.OutputResults[0].ResultSets[1].Name.Should().Be("Cumulative");
            result.OutputResults[0].ResultSets[1].DataSeries.Count.Should().Be(1);
            result.OutputResults[0].ResultSets[1].DataSeries[0].Name.Should().Be("WEL");
            result.OutputResults[0].ResultSets[1].DataSeries[0].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[1].DataSeries[0].DataPoints[0].Value.Should().Be(0);

            TestListFileResult(result.OutputResults[1], listFileData);
        }

        [TestMethod]
        public void GenerateListFileOutput_NonDifferential_RepeatingPackages_NonZeroResults()
        {
            var totalInFirst = "0.00";
            var totalInSecond = "1.00";
            var stepInFirst = "0.00";
            var stepInSecond = "2.00";
            var totalOutFirst = "5.00";
            var totalOutSecond = "6.00";
            var stepOutFirst = "3.00";
            var stepOutSecond = "5.00";
            var totalLast = "1.01";
            var stepLast = "0.00";

            var listFileData = new List<string>
            {
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   1",
                $" WEL =           {totalInFirst}     WEL =          {stepInFirst}",
                $" WEL =           {totalInSecond}     WEL =          {stepInSecond}",
                $" TOTAL IN =           {0}     TOTAL IN =           {0}",
                $" WEL =           {totalOutFirst}     WEL =          {stepOutFirst}",
                $" WEL =           {totalOutSecond}     WEL =          {stepOutSecond}",
                $" PERCENT DISCREPANCY =           {totalInFirst}     PERCENT DISCREPANCY =          {stepInFirst}",
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   2",
                $" WEL =           {totalInFirst}     WEL =          {stepInFirst}",
                $" WEL =           {totalInSecond}     WEL =          {stepInSecond}",
                $" TOTAL IN =           {0}     TOTAL IN =           {0}",
                $" WEL =           {totalOutFirst}     WEL =          {stepOutFirst}",
                $" WEL =           {totalOutSecond}     WEL =          {stepOutSecond}",
                $" PERCENT DISCREPANCY =           {totalLast}     PERCENT DISCREPANCY =           {stepLast}",
            };
            _modflowFileAccessorMock.Arrange(a => a.GetRunListFileLines())
                .Returns(listFileData);
            _modflowFileAccessorMock.Arrange(a => a.GetListFileOutputFileLines())
                .Returns(listFileData);

            _modflowFileAccessorMock.Arrange(a => a.GetAsrDataNameMap())
                .Returns(new List<AsrDataMap>
                {
                    new AsrDataMap { Key = "WEL", Name = "WEL" }
                });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedZoneBudget(false)).Returns((IEnumerable<ObservedZoneBudgetData>)null);

            var sut = CreateListFileOutputSubEngine();
            var result = sut.GenerateListFileOutput(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, false);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOfType(result.Exception, typeof(OutputDataInvalidException));
            Assert.IsNotNull(result.OutputResults);
            Assert.AreEqual(2, result.OutputResults.Count);
            result.OutputResults[0].RunResultName.Should().Be("Water Budget");
            result.OutputResults[0].ResultSets.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].Name.Should().Be("Rate");
            result.OutputResults[0].ResultSets[0].DataSeries.Count.Should().Be(1);
            result.OutputResults[0].ResultSets[0].DataSeries[0].Name.Should().Be("WEL");
            result.OutputResults[0].ResultSets[0].DataSeries[0].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].DataSeries[0].DataPoints[0].Value.Should().Be(-0.004269972451790633);
            result.OutputResults[0].ResultSets[1].Name.Should().Be("Cumulative");
            result.OutputResults[0].ResultSets[1].DataSeries.Count.Should().Be(1);
            result.OutputResults[0].ResultSets[1].DataSeries[0].Name.Should().Be("WEL");
            result.OutputResults[0].ResultSets[1].DataSeries[0].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[1].DataSeries[0].DataPoints[0].Value.Should().Be(-0.004269972451790633);

            TestListFileResult(result.OutputResults[1], listFileData);
        }

        [TestMethod]
        public void GenerateListFileOutput_NonDifferential_RepeatingPackagesAndNonRepeatingPackages()
        {
            var totalInFirst = "0.00";
            var totalInSecond = "1.00";
            var stepInFirst = "0.00";
            var stepInSecond = "2.00";
            var totalOutFirst = "0.00";
            var totalOutSecond = "1.00";
            var stepOutFirst = "0.00";
            var stepOutSecond = "2.00";
            var totalLast = "1.01";
            var stepLast = "0.00";

            var listFileData = new List<string>
            {
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   1",
                $" BEL =           {totalInFirst}     BEL =          {stepInFirst}",
                $" WEL =           {totalInFirst}     WEL =          {stepInFirst}",
                $" WEL =           {totalInSecond}     WEL =          {stepInSecond}",
                $" TOTAL IN =           {0}     TOTAL IN =           {0}",
                $" BEL =           {totalInFirst}     BEL =          {stepInFirst}",
                $" WEL =           {totalOutFirst}     WEL =          {stepOutFirst}",
                $" WEL =           {totalOutSecond}     WEL =          {stepOutSecond}",
                $" PERCENT DISCREPANCY =           {totalInFirst}     PERCENT DISCREPANCY =          {stepInFirst}",
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   2",
                $" BEL =           {totalInFirst}     BEL =          {stepInFirst}",
                $" WEL =           {totalInFirst}     WEL =          {stepInFirst}",
                $" WEL =           {totalInSecond}     WEL =          {stepInSecond}",
                $" TOTAL IN =           {0}     TOTAL IN =           {0}",
                $" BEL =           {totalInFirst}     BEL =          {stepInFirst}",
                $" WEL =           {totalOutFirst}     WEL =          {stepOutFirst}",
                $" WEL =           {totalOutSecond}     WEL =          {stepOutSecond}",
                $" PERCENT DISCREPANCY =           {totalLast}     PERCENT DISCREPANCY =           {stepLast}",
            };
            _modflowFileAccessorMock.Arrange(a => a.GetRunListFileLines())
                .Returns(listFileData);
            _modflowFileAccessorMock.Arrange(a => a.GetListFileOutputFileLines())
                .Returns(listFileData);

            _modflowFileAccessorMock.Arrange(a => a.GetAsrDataNameMap())
                .Returns(new List<AsrDataMap>
                {
                    new AsrDataMap { Key = "WEL", Name = "WEL" },
                    new AsrDataMap { Key = "BEL", Name = "BEL"}
                });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedZoneBudget(false)).Returns((IEnumerable<ObservedZoneBudgetData>)null);

            var sut = CreateListFileOutputSubEngine();
            var result = sut.GenerateListFileOutput(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, false);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOfType(result.Exception, typeof(OutputDataInvalidException));
            Assert.IsNotNull(result.OutputResults);
            Assert.AreEqual(2, result.OutputResults.Count);
            result.OutputResults[0].RunResultName.Should().Be("Water Budget");
            result.OutputResults[0].ResultSets.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].Name.Should().Be("Rate");
            result.OutputResults[0].ResultSets[0].DataSeries.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].DataSeries[0].Name.Should().Be("BEL");
            result.OutputResults[0].ResultSets[0].DataSeries[0].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].DataSeries[0].DataPoints[0].Value.Should().Be(0);
            result.OutputResults[0].ResultSets[0].DataSeries[1].Name.Should().Be("WEL");
            result.OutputResults[0].ResultSets[0].DataSeries[1].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].DataSeries[1].DataPoints[0].Value.Should().Be(0);
            result.OutputResults[0].ResultSets[1].Name.Should().Be("Cumulative");
            result.OutputResults[0].ResultSets[1].DataSeries.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[1].DataSeries[0].Name.Should().Be("BEL");
            result.OutputResults[0].ResultSets[1].DataSeries[0].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[1].DataSeries[0].DataPoints[0].Value.Should().Be(0);
            result.OutputResults[0].ResultSets[1].DataSeries[1].Name.Should().Be("WEL");
            result.OutputResults[0].ResultSets[1].DataSeries[1].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[1].DataSeries[1].DataPoints[0].Value.Should().Be(0);

            TestListFileResult(result.OutputResults[1], listFileData);
        }

        [TestMethod]
        public void GenerateListFileOutput_Observed()
        {
            var totalFirst = "0.00";
            var stepFirst = "0.00";
            var totalLast = "1.01";
            var stepLast = "0.00";

            var listFileData = new List<string>
            {
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   1",
                $" AAA =           {totalFirst}     AAA =          {stepFirst}",
                $" PERCENT DISCREPANCY =           {totalFirst}     PERCENT DISCREPANCY =          {stepFirst}",
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   1",
                $" AAA =           {totalLast}     AAA =           {stepLast}",
                $" PERCENT DISCREPANCY =           {totalLast}     PERCENT DISCREPANCY =           {stepLast}",
            };
            _modflowFileAccessorMock.Arrange(a => a.GetRunListFileLines())
                .Returns(listFileData);
            _modflowFileAccessorMock.Arrange(a => a.GetListFileOutputFileLines()).Returns(listFileData);

            _modflowFileAccessorMock.Arrange(a => a.GetAsrDataNameMap())
                .Returns(new List<AsrDataMap>
                {
                    new AsrDataMap { Key = "AAA", Name = "AAA" }
                });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedZoneBudget(false)).Returns(new List<ObservedZoneBudgetData>
            {
                new ObservedZoneBudgetData
                {
                    BudgetItemSeriesName = "AAA - Observed",
                    ZoneSeriesName = "Zone A",
                    Period = 1,
                    ValueInAcreFeet = 1.25
                },
                new ObservedZoneBudgetData
                {
                    BudgetItemSeriesName = "AAA - Observed",
                    ZoneSeriesName = "Zone A",
                    Period = 2,
                    ValueInAcreFeet = 1.5
                },
            });

            var sut = CreateListFileOutputSubEngine();
            var result = sut.GenerateListFileOutput(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, false);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOfType(result.Exception, typeof(OutputDataInvalidException));
            Assert.IsNotNull(result.OutputResults);
            Assert.AreEqual(2, result.OutputResults.Count);
            result.OutputResults[0].RunResultName.Should().Be("Water Budget");
            result.OutputResults[0].ResultSets.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].Name.Should().Be("Rate");
            result.OutputResults[0].ResultSets[0].DataSeries.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].DataSeries[0].Name.Should().Be("AAA");
            result.OutputResults[0].ResultSets[0].DataSeries[0].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[0].DataSeries[1].Name.Should().Be("AAA - Observed");
            result.OutputResults[0].ResultSets[0].DataSeries[1].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[1].Name.Should().Be("Cumulative");
            result.OutputResults[0].ResultSets[1].DataSeries.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[1].DataSeries[0].Name.Should().Be("AAA");
            result.OutputResults[0].ResultSets[1].DataSeries[0].DataPoints.Count.Should().Be(2);
            result.OutputResults[0].ResultSets[1].DataSeries[1].Name.Should().Be("AAA - Observed");
            result.OutputResults[0].ResultSets[1].DataSeries[1].DataPoints.Count.Should().Be(2);

            TestListFileResult(result.OutputResults[1], listFileData);
        }

        private void PercentDiscrepancyOutOfRange(string totalFirst, string stepFirst, string totalLast, string stepLast)
        {
            var listFileData = new List<string>
            {
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   1",
                $" PERCENT DISCREPANCY =           {totalFirst}     PERCENT DISCREPANCY =          {stepFirst}",
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    2, STRESS PERIOD   1",
                $" PERCENT DISCREPANCY =           {totalLast}     PERCENT DISCREPANCY =           {stepLast}"
            };

            _modflowFileAccessorMock.Arrange(a => a.GetRunListFileLines())
                .Returns(listFileData);
            _modflowFileAccessorMock.Arrange(a => a.GetListFileOutputFileLines())
                .Returns(listFileData);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineListFileLines())
                .Returns(listFileData);
            _modflowFileAccessorMock.Arrange(a => a.GetObservedZoneBudget(true)).Returns((List<ObservedZoneBudgetData>)null);

            _modflowFileAccessorMock.Arrange(a => a.GetAsrDataNameMap())
                .Returns(new List<AsrDataMap>
                {
                    new AsrDataMap { Key = "AAA", Name = "AAA" }
                });

            var sut = CreateListFileOutputSubEngine();
            var result = sut.GenerateListFileOutput(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOfType(result.Exception, typeof(OutputDataInvalidException));
            Assert.IsNotNull(result.OutputResults);
            Assert.AreEqual(2, result.OutputResults.Count);
            TestListFileResult(result.OutputResults[1], listFileData);
        }

        private void PercentDiscrepancyInRange(string totalFirst, string stepFirst, string totalLast, string stepLast)
        {
            var listFileData = new List<string>
            {
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    1, STRESS PERIOD   1",
                $" PERCENT DISCREPANCY =           {totalFirst}     PERCENT DISCREPANCY =          {stepFirst}",
                $"  VOLUMETRIC BUDGET FOR ENTIRE MODEL AT END OF TIME STEP    2, STRESS PERIOD   1",
                $" PERCENT DISCREPANCY =           {totalLast}     PERCENT DISCREPANCY =           {stepLast}"
            };
            _modflowFileAccessorMock.Arrange(a => a.GetRunListFileLines())
                .Returns(listFileData);
            _modflowFileAccessorMock.Arrange(a => a.GetListFileOutputFileLines())
                .Returns(listFileData);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineListFileLines())
                .Returns(listFileData);
            _modflowFileAccessorMock.Arrange(a => a.GetObservedZoneBudget(true)).Returns((List<ObservedZoneBudgetData>)null);

            _modflowFileAccessorMock.Arrange(a => a.GetAsrDataNameMap())
                .Returns(new List<AsrDataMap>
                {
                    new AsrDataMap { Key = "AAA", Name = "AAA" }
                });

            var sut = CreateListFileOutputSubEngine();
            var result = sut.GenerateListFileOutput(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.IsNull(result.Exception);
            Assert.IsNotNull(result.OutputResults);
            Assert.AreEqual(2, result.OutputResults.Count);
            TestListFileResult(result.OutputResults[1], listFileData);
        }

        private void TestListFileResult(RunResultDetails totalResult, IEnumerable<string> text)
        {
            totalResult.Should().NotBeNull();
            totalResult.RunResultName.Should().Be("List File Output");
            totalResult.ResultSets.Should().NotBeNull().And.Subject.Count().Should().Be(1);
            totalResult.ResultSets[0].Name.Should().Be("List File Output");
            totalResult.ResultSets[0].DisplayType.Should().Be(RunResultDisplayType.Text);
            totalResult.ResultSets[0].DataSeries.Should().BeNull();
            totalResult.ResultSets[0].TextDisplay.Should().NotBeNull();
            totalResult.ResultSets[0].TextDisplay.FileName.Should().Be("ListFile.txt");
            totalResult.ResultSets[0].TextDisplay.Text.Should().Be(string.Join(Environment.NewLine, text) + Environment.NewLine);
        }

        private readonly IModelFileAccessor _modflowFileAccessorMock = Mock.Create<IModelFileAccessor>(Behavior.Strict);

        private ListFileOutputSubEngine CreateListFileOutputSubEngine()
        {
            _model.NumberOfStressPeriods = _getStressPeriodDataResult.Count;
            return new ListFileOutputSubEngine(_model);
        }

        private ListFileOutputSubEngine CreateListFileOutputSubEngineAnnual()
        {
            _model.NumberOfStressPeriods = _getStressPeriodDataResult.Count;
            _model.ModelStressPeriodCustomStartDates = new List<ModelStressPeriodCustomStartDate>
            {
                new ModelStressPeriodCustomStartDate()
                {
                    StressPeriod = 1,
                    StressPeriodStartDate = new DateTime(2011, 1,  1)
                }, 
                new ModelStressPeriodCustomStartDate() {
                    StressPeriodStartDate = new DateTime(2012, 1, 1),
                    StressPeriod = 2
                }

            };
            return new ListFileOutputSubEngine(_model);
        }
    }
}
