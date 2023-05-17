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

namespace Olsson.GET.Tests.EngineTests.ModelInputOutputEngines
{
    [TestClass]
    public class ZoneBudgetOutputSubEngineTests
    {
        private readonly IModelFileAccessor _modflowFileAccessorMock = Mock.Create<IModelFileAccessor>();
        private List<string> _zones;
        private List<ZoneBudgetItem> _runZoneBudgetData;
        private List<ZoneBudgetItem> _baselineZoneBudgetData;
        private List<StressPeriod> _stressPeriods;
        private List<AsrDataMap> _asrDataMap;
        private List<ObservedZoneBudgetData> _observedData;

        [TestInitialize]
        public void TestInitialize()
        {
            _stressPeriods = GetStressPeriods();

            _zones = new List<string> { "b", "a", "c" };
            _asrDataMap = new List<AsrDataMap>
            {
                new AsrDataMap{Key = "KeyA", Name = "NameA"},
                new AsrDataMap{Key = "KeyC", Name = "NameC"},
                new AsrDataMap{Key = "KeyB", Name = "NameB"},
                new AsrDataMap{Key = "KeyD", Name = "NameD"}
            };

            _runZoneBudgetData = CreateTestOutputData();
            _baselineZoneBudgetData = CreateTestOutputData();

            _modflowFileAccessorMock.Arrange(a => a.GetRunZoneBudgetItems(_asrDataMap))
                .Returns(() => _runZoneBudgetData);

            _modflowFileAccessorMock.Arrange(a => a.GetBaselineZoneBudgetItems(_asrDataMap))
                .Returns(() => _baselineZoneBudgetData);

            _modflowFileAccessorMock.Arrange(a => a.GetZoneBudgetAsrDataNameMap())
                .Returns(() => _asrDataMap);

            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyZoneBudgetName(Arg.AnyString))
                .Returns((a) => $"{a}{a.ToUpper()}{a}");

            _modflowFileAccessorMock.Arrange(a => a.GetObservedZoneBudget(Arg.AnyBool))
                .Returns(() => _observedData);
        }

        private List<ZoneBudgetItem> CreateTestOutputData()
        {
            var result = new List<ZoneBudgetItem>();

            for (var i = 0; i < _stressPeriods.Count; i++)
            {
                for (var j = 0; j < _stressPeriods[i].NumberOfTimeSteps; j++)
                {
                    for (var k = 0; k < _zones.Count; k++)
                    {
                        result.Add(new ZoneBudgetItem
                        {
                            Period = i + 1,
                            Step = j + 1,
                            Zone = _zones[k],
                            Values = _asrDataMap.SelectMany(a => new[]
                            {
                                new ZoneBudgetValue { Key = a.Key, Value = i | j | k | 1 },
                                new ZoneBudgetValue { Key = a.Key, Value = i | j | k | 2 }
                            }).ToList()
                        });
                    }

                }
            }

            return result;
        }

        private ZoneBudgetOutputSubEngine CreateZoneBudgetOutputSubEngine()
        {
            return new ZoneBudgetOutputSubEngine(new Model { StartDateTime = new DateTime(2017, 1, 1), NumberOfStressPeriods = _stressPeriods.Count });
        }

        private List<StressPeriod> GetStressPeriods()
        {
            return new List<StressPeriod>
            {
                new StressPeriod{Days = 31, NumberOfTimeSteps = 2},
                new StressPeriod{Days = 28, NumberOfTimeSteps = 2}
            };
        }

        [TestMethod]
        public void CreateZoneBudgetOutputResults_NoChanges()
        {
            var sut = CreateZoneBudgetOutputSubEngine();
            var result = sut.CreateZoneBudgetOutputResults(_modflowFileAccessorMock, _stressPeriods, VolumeUnit.AcreFeet.VolumeUnitID, true);
            
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("Water Budget By Zone", result[0].SetName);
            Assert.AreEqual(3, result[0].RelatedResults.Count);
            Assert.AreEqual(2, result[0].RelatedResults[0].ResultSets.Count);
            Assert.AreEqual("aAa", result[0].RelatedResults[0].RunResultName);
            Assert.AreEqual("Rate", result[0].RelatedResults[0].ResultSets[0].Name);
            Assert.AreEqual(RunResultDisplayType.LineChart, result[0].RelatedResults[0].ResultSets[0].DisplayType);
            Assert.IsNull(result[0].RelatedResults[0].ResultSets[0].MapData);
            Assert.IsNull(result[0].RelatedResults[0].ResultSets[0].TextDisplay);
            Assert.AreEqual(4, result[0].RelatedResults[0].ResultSets[0].DataSeries.Count);
            Assert.AreEqual("NameA", result[0].RelatedResults[0].ResultSets[0].DataSeries[0].Name);
            Assert.AreEqual("NameB", result[0].RelatedResults[0].ResultSets[0].DataSeries[1].Name);
            Assert.AreEqual("NameC", result[0].RelatedResults[0].ResultSets[0].DataSeries[2].Name);
            Assert.AreEqual("NameD", result[0].RelatedResults[0].ResultSets[0].DataSeries[3].Name);
            foreach (var dataSeries in result[0].RelatedResults[0].ResultSets[0].DataSeries)
            {
                Assert.IsFalse(dataSeries.IsDefaultDisplayed);
                Assert.AreEqual(2, dataSeries.DataPoints.Count);
                Assert.AreEqual(new DateTime(2017, 1, 1), dataSeries.DataPoints[0].Date);
                Assert.AreEqual(0.0, dataSeries.DataPoints[0].Value, .0001);
                Assert.AreEqual(new DateTime(2017, 2, 1), dataSeries.DataPoints[1].Date);
                Assert.AreEqual(0.0, dataSeries.DataPoints[1].Value, .0001);
            }

            Assert.AreEqual("Water Budget By Budget Item", result[1].SetName);
            Assert.AreEqual(4, result[1].RelatedResults.Count);
            Assert.AreEqual(2, result[1].RelatedResults[0].ResultSets.Count);
            Assert.AreEqual("NameA", result[1].RelatedResults[0].RunResultName);
            Assert.AreEqual("Rate", result[1].RelatedResults[0].ResultSets[0].Name);
            Assert.AreEqual(RunResultDisplayType.LineChart, result[1].RelatedResults[0].ResultSets[0].DisplayType);
            Assert.IsNull(result[1].RelatedResults[0].ResultSets[0].MapData);
            Assert.IsNull(result[1].RelatedResults[0].ResultSets[0].TextDisplay);
            Assert.AreEqual(3, result[1].RelatedResults[0].ResultSets[0].DataSeries.Count);
            Assert.AreEqual("aAa", result[1].RelatedResults[0].ResultSets[0].DataSeries[0].Name);
            Assert.AreEqual("bBb", result[1].RelatedResults[0].ResultSets[0].DataSeries[1].Name);
            Assert.AreEqual("cCc", result[1].RelatedResults[0].ResultSets[0].DataSeries[2].Name);
            foreach (var dataSeries in result[1].RelatedResults[0].ResultSets[0].DataSeries)
            {
                Assert.IsFalse(dataSeries.IsDefaultDisplayed);
                Assert.AreEqual(2, dataSeries.DataPoints.Count);
                Assert.AreEqual(new DateTime(2017, 1, 1), dataSeries.DataPoints[0].Date);
                Assert.AreEqual(0.0, dataSeries.DataPoints[0].Value, .0001);
                Assert.AreEqual(new DateTime(2017, 2, 1), dataSeries.DataPoints[1].Date);
                Assert.AreEqual(0.0, dataSeries.DataPoints[1].Value, .0001);
            }
        }

        [TestMethod]
        public void CreateZoneBudgetOutputResults_AsrData_Null()
        {
            _asrDataMap = null;

            var sut = CreateZoneBudgetOutputSubEngine();
            var result = sut.CreateZoneBudgetOutputResults(_modflowFileAccessorMock, _stressPeriods, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CreateZoneBudgetOutputResults_AsrData_Empty()
        {
            _asrDataMap.Clear();

            var sut = CreateZoneBudgetOutputSubEngine();
            var result = sut.CreateZoneBudgetOutputResults(_modflowFileAccessorMock, _stressPeriods, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CreateZoneBudgetOutputResults_BaselineData_Null()
        {
            _baselineZoneBudgetData = null;

            var sut = CreateZoneBudgetOutputSubEngine();
            var result = sut.CreateZoneBudgetOutputResults(_modflowFileAccessorMock, _stressPeriods, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CreateZoneBudgetOutputResults_RunData_Null()
        {
            _runZoneBudgetData = null;

            var sut = CreateZoneBudgetOutputSubEngine();
            var result = sut.CreateZoneBudgetOutputResults(_modflowFileAccessorMock, _stressPeriods, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CreateZoneBudgetOutputResults_NonDifferential()
        {
            _baselineZoneBudgetData = null;

            var sut = CreateZoneBudgetOutputSubEngine();
            var result = sut.CreateZoneBudgetOutputResults(_modflowFileAccessorMock, _stressPeriods, VolumeUnit.AcreFeet.VolumeUnitID, false);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("Water Budget By Zone", result[0].SetName);
            Assert.AreEqual(3, result[0].RelatedResults.Count);
            Assert.AreEqual(2, result[0].RelatedResults[0].ResultSets.Count);
            Assert.AreEqual("aAa", result[0].RelatedResults[0].RunResultName);
            Assert.AreEqual("Rate", result[0].RelatedResults[0].ResultSets[0].Name);
            Assert.AreEqual(RunResultDisplayType.LineChart, result[0].RelatedResults[0].ResultSets[0].DisplayType);
            Assert.IsNull(result[0].RelatedResults[0].ResultSets[0].MapData);
            Assert.IsNull(result[0].RelatedResults[0].ResultSets[0].TextDisplay);
            Assert.AreEqual(4, result[0].RelatedResults[0].ResultSets[0].DataSeries.Count);
            Assert.AreEqual("NameA", result[0].RelatedResults[0].ResultSets[0].DataSeries[0].Name);
            Assert.AreEqual("NameB", result[0].RelatedResults[0].ResultSets[0].DataSeries[1].Name);
            Assert.AreEqual("NameC", result[0].RelatedResults[0].ResultSets[0].DataSeries[2].Name);
            Assert.AreEqual("NameD", result[0].RelatedResults[0].ResultSets[0].DataSeries[3].Name);
            foreach (var dataSeries in result[0].RelatedResults[0].ResultSets[0].DataSeries)
            {
                Assert.IsTrue(dataSeries.IsDefaultDisplayed);
                Assert.AreEqual(2, dataSeries.DataPoints.Count);
                Assert.AreEqual(new DateTime(2017, 1, 1), dataSeries.DataPoints[0].Date);
                Assert.AreEqual(-0.00142, dataSeries.DataPoints[0].Value, .0001);
                Assert.AreEqual(new DateTime(2017, 2, 1), dataSeries.DataPoints[1].Date);
                Assert.AreEqual(-0.00129, dataSeries.DataPoints[1].Value, .0001);
            }

            Assert.AreEqual("Water Budget By Budget Item", result[1].SetName);
            Assert.AreEqual(4, result[1].RelatedResults.Count);
            Assert.AreEqual(2, result[1].RelatedResults[0].ResultSets.Count);
            Assert.AreEqual("NameA", result[1].RelatedResults[0].RunResultName);
            Assert.AreEqual("Rate", result[1].RelatedResults[0].ResultSets[0].Name);
            Assert.AreEqual(RunResultDisplayType.LineChart, result[1].RelatedResults[0].ResultSets[0].DisplayType);
            Assert.IsNull(result[1].RelatedResults[0].ResultSets[0].MapData);
            Assert.IsNull(result[1].RelatedResults[0].ResultSets[0].TextDisplay);
            Assert.AreEqual(3, result[1].RelatedResults[0].ResultSets[0].DataSeries.Count);
            Assert.AreEqual("aAa", result[1].RelatedResults[0].ResultSets[0].DataSeries[0].Name);
            Assert.AreEqual("bBb", result[1].RelatedResults[0].ResultSets[0].DataSeries[1].Name);
            Assert.AreEqual("cCc", result[1].RelatedResults[0].ResultSets[0].DataSeries[2].Name);

            var dataSeries2 = result[1].RelatedResults[0].ResultSets[0].DataSeries[0];
            Assert.IsTrue(dataSeries2.IsDefaultDisplayed);
            Assert.AreEqual(2, dataSeries2.DataPoints.Count);
            Assert.AreEqual(new DateTime(2017, 1, 1), dataSeries2.DataPoints[0].Date);
            Assert.AreEqual(-0.00142, dataSeries2.DataPoints[0].Value, .0001);
            Assert.AreEqual(new DateTime(2017, 2, 1), dataSeries2.DataPoints[1].Date);
            Assert.AreEqual(-0.00129, dataSeries2.DataPoints[1].Value, .0001);

            dataSeries2 = result[1].RelatedResults[0].ResultSets[0].DataSeries[1];
            Assert.IsTrue(dataSeries2.IsDefaultDisplayed);
            Assert.AreEqual(2, dataSeries2.DataPoints.Count);
            Assert.AreEqual(new DateTime(2017, 1, 1), dataSeries2.DataPoints[0].Date);
            Assert.AreEqual(-0.00107, dataSeries2.DataPoints[0].Value, .0001);
            Assert.AreEqual(new DateTime(2017, 2, 1), dataSeries2.DataPoints[1].Date);
            Assert.AreEqual(-0.00129, dataSeries2.DataPoints[1].Value, .0001);

            dataSeries2 = result[1].RelatedResults[0].ResultSets[0].DataSeries[2];
            Assert.IsTrue(dataSeries2.IsDefaultDisplayed);
            Assert.AreEqual(2, dataSeries2.DataPoints.Count);
            Assert.AreEqual(new DateTime(2017, 1, 1), dataSeries2.DataPoints[0].Date);
            Assert.AreEqual(0.00036, dataSeries2.DataPoints[0].Value, .0001);
            Assert.AreEqual(new DateTime(2017, 2, 1), dataSeries2.DataPoints[1].Date);
            Assert.AreEqual(0, dataSeries2.DataPoints[1].Value, .0001);

        }

        [TestMethod]
        public void CreateZoneBudgetOutputResults_ObservedData()
        {
            _observedData = new List<ObservedZoneBudgetData>
            {
                new ObservedZoneBudgetData { BudgetItemSeriesName = "Recharge - Observed", Period = 1, ZoneSeriesName = "Zone A - Observed", ValueInAcreFeet = .0001 }
            };

            var sut = CreateZoneBudgetOutputSubEngine();
            var result = sut.CreateZoneBudgetOutputResults(_modflowFileAccessorMock, _stressPeriods, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            result[0].SetName.Should().Be("Water Budget By Zone");
            result[0].RelatedResults.Count.Should().Be(4);
            result[0].RelatedResults[3].RunResultName.Should().Be("Zone A - Observed");
            result[0].RelatedResults[3].ResultSets.Count.Should().Be(2);
            result[0].RelatedResults[3].ResultSets[0].Name.Should().Be("Rate");
            result[0].RelatedResults[3].ResultSets[0].DataSeries.Count.Should().Be(1);
            result[0].RelatedResults[3].ResultSets[0].DataSeries[0].DataPoints.Count.Should().Be(1);
            result[0].RelatedResults[3].ResultSets[0].DataSeries[0].DataPoints[0].Value.Should().Be(.00155);
            result[0].RelatedResults[3].ResultSets[1].Name.Should().Be("Cumulative");
            result[0].RelatedResults[3].ResultSets[1].DataSeries.Count.Should().Be(1);
            result[0].RelatedResults[3].ResultSets[1].DataSeries[0].DataPoints.Count.Should().Be(1);
            result[0].RelatedResults[3].ResultSets[1].DataSeries[0].DataPoints[0].Value.Should().Be(.00155);

            result[1].SetName.Should().Be("Water Budget By Budget Item");
            result[1].RelatedResults.Count.Should().Be(5);
            result[1].RelatedResults[4].RunResultName.Should().Be("Recharge - Observed");
            result[1].RelatedResults[4].ResultSets.Count.Should().Be(2);
            result[1].RelatedResults[4].ResultSets[0].Name.Should().Be("Rate");
            result[1].RelatedResults[4].ResultSets[0].DataSeries.Count.Should().Be(1);
            result[1].RelatedResults[4].ResultSets[0].DataSeries[0].DataPoints.Count.Should().Be(1);
            result[1].RelatedResults[4].ResultSets[0].DataSeries[0].DataPoints[0].Value.Should().Be(.00155);
            result[1].RelatedResults[4].ResultSets[1].Name.Should().Be("Cumulative");
            result[1].RelatedResults[4].ResultSets[1].DataSeries.Count.Should().Be(1);
            result[1].RelatedResults[4].ResultSets[1].DataSeries[0].DataPoints.Count.Should().Be(1);
            result[1].RelatedResults[4].ResultSets[1].DataSeries[0].DataPoints[0].Value.Should().Be(.00155);
        }
    }
}
