using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Engines.ModelInputOutputEngines;
using Olsson.GET.Accessors.Runs;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using FluentAssertions;
using Olsson.GET.Accessors.EntityFramework;
using Model = Olsson.GET.Common.DataContracts.Models.Model;

namespace Olsson.GET.Tests.EngineTests.ModelInputOutputEngines
{
    [TestClass]
    public class LocationMapSubEngineTests
    {
        private readonly IModelFileAccessor _modflowFileAccessorMock = Mock.Create<IModelFileAccessor>();
        private List<string> _locationPositionMap;
        private List<string> _badLocationPositionMap;
        private string _outputLocationGeoJSONRaw;
        private string _badOutputLocationGeoJSONRaw;
        private List<MapOutputData> _mapOutputData;
        private List<MapOutputData> _mapDrawdownData;
        private List<MapOutputData> _mapBaselineData;

        private LocationMapOutputSubEngine CreateLocationMapSubEngine()
        {
            return new LocationMapOutputSubEngine(new Model { StartDateTime = new DateTime(2017, 1, 1) });
        }

        private List<StressPeriod> GetStressPeriods()
        {
            return new List<StressPeriod>
            {
                new StressPeriod{Days = 31, NumberOfTimeSteps = 2},
                new StressPeriod{Days = 28, NumberOfTimeSteps = 2}
            };
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _locationPositionMap = new List<string> {"a", "b", "c", "d"};
            _badLocationPositionMap = new List<string> {"a", "b", "c"};
            _outputLocationGeoJSONRaw = "[{\"ZoneNumber\":\"a\",\"Name\":\"A\", \"Bounds\":[]},{\"ZoneNumber\":\"b\",\"Name\":\"B\", \"Bounds\":[]},{\"ZoneNumber\":\"c\",\"Name\":\"C\", \"Bounds\":[]},{\"ZoneNumber\":\"d\",\"Name\":\"D\", \"Bounds\":[]}]";
            _badOutputLocationGeoJSONRaw = "[{\"ZoneNumber\":\"b\",\"Name\":\"B\", \"Bounds\":[]},{\"ZoneNumber\":\"c\",\"Name\":\"C\", \"Bounds\":[]},{\"ZoneNumber\":\"d\",\"Name\":\"D\", \"Bounds\":[]}]";
        }

        private List<MapOutputData> CreateTestMapOutputData(List<StressPeriod> stressPeriods, double multiplier = 1)
        {
            var result = new List<MapOutputData>();
            for (var j = 0; j < stressPeriods.Count; j++)
            {
                for (var i = 0; i < stressPeriods[j].NumberOfTimeSteps; i++)
                {
                    foreach (var location in _locationPositionMap)
                    {
                        result.Add(new MapOutputData
                        {
                            Location = location,
                            StressPeriod = j + 1,
                            TimeStep = i + 1,
                            Value = (j | i) * multiplier
                        });
                    }
                }
            }
            return result;
        }

        [TestMethod]
        public void CreateWaterLevelHeatMapResults_HasRanDryNodes()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods);
            _mapBaselineData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.GetBaselineMapData())
                .Returns(() => _mapBaselineData);

            _mapOutputData.Single(a => a.StressPeriod == 2 && a.TimeStep == 2 && a.Location == "c").Value = null;
            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateWaterLevelHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), true);

            Assert.IsNotNull(result.Exception);
            Assert.AreEqual((int) RunStatusEnum.HasDryCells, result.Exception.Status);

            Assert.IsNotNull(result.OutputResults);
            Assert.IsNotNull(result.OutputResults.RelatedResults);
            foreach (var resultSet in result.OutputResults.RelatedResults)
            {
                Assert.IsNotNull(resultSet.ResultSets[0].MapData.Legend.FirstOrDefault(a => a.Text == "Ran Dry"));
            }
        }

        [TestMethod]
        public void CreateWaterLevelHeatMapResults_WasDryNodes()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods);
            _mapBaselineData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.GetBaselineMapData())
                .Returns(() => _mapBaselineData);

            _mapBaselineData.Single(a => a.StressPeriod == 2 && a.TimeStep == 2 && a.Location == "c").Value = null;
            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateWaterLevelHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), true);

            Assert.IsNull(result.Exception);

            Assert.IsNotNull(result.OutputResults);
            Assert.IsNotNull(result.OutputResults.RelatedResults);
            foreach (var resultSet in result.OutputResults.RelatedResults)
            {
                Assert.IsNotNull(resultSet.ResultSets[0].MapData.Legend.FirstOrDefault(a => a.Text == "Was Dry"));
            }
        }

        [TestMethod]
        public void CreateWaterLevelHeatMapResults_NoDataResultIfNoStressPeriodData()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods);
            _mapBaselineData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.GetBaselineMapData())
                .Returns(() => _mapBaselineData);

            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateWaterLevelHeatMapResults(_modflowFileAccessorMock, new List<StressPeriod>(), true);

            Assert.IsNull(result.Exception);

            Assert.IsNull(result.OutputResults);
        }

        [TestMethod]
        public void CreateWaterLevelHeatMapResults_NoLegendIfNoChanges()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods);
            _mapBaselineData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.GetBaselineMapData())
                .Returns(() => _mapBaselineData);

            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateWaterLevelHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), true);

            Assert.IsNull(result.Exception);

            Assert.IsNotNull(result.OutputResults);
            Assert.IsNotNull(result.OutputResults.RelatedResults);
            foreach (var resultSet in result.OutputResults.RelatedResults)
            {
                Assert.IsNull(resultSet.ResultSets[0].MapData.Legend);
            }
        }

        [TestMethod]
        public void CreateWaterLevelHeatMapResults_OutOfOrder()
        {
            var stressPeriods = GetStressPeriods();

            // The data must be aligned (incorrectly) or we'll error on mismatched data before out of order data
            _mapOutputData = CreateTestMapOutputData(stressPeriods).OrderByDescending(x => x.StressPeriod).ToList();
            _mapBaselineData = CreateTestMapOutputData(stressPeriods).OrderByDescending(x => x.StressPeriod).ToList();

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.GetBaselineMapData())
                .Returns(() => _mapBaselineData);

            _mapBaselineData.Single(a => a.StressPeriod == 2 && a.TimeStep == 2 && a.Location == "c").Value = null;
            var sut = CreateLocationMapSubEngine();
            Action action = () => sut.CreateWaterLevelHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), true);
            action.Should().Throw<Exception>().WithMessage("Data is out of order.");
        }

        [TestMethod]
        public void CreateWaterLevelHeatMapResults_NonDifferentialRun()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateWaterLevelHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), false);

            Assert.IsNull(result.Exception);

            Assert.IsNotNull(result.OutputResults);
            Assert.IsNotNull(result.OutputResults.RelatedResults);
        }

        [TestMethod]
        public void CreateWaterLevelHeatMapResults_NonDifferentialRun_OutOfOrder()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods).OrderByDescending(x => x.StressPeriod).ToList();

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            var sut = CreateLocationMapSubEngine();
            Action action = () => sut.CreateWaterLevelHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), false);

            action.Should().Throw<Exception>().WithMessage("Data is out of order.");
        }

        [TestMethod]
        public void CreateDrawdownHeatMapResults()
        {
            var stressPeriods = GetStressPeriods();

            _mapDrawdownData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetDrawdownMapData())
                .Returns(() => _mapDrawdownData);

            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateDrawdownHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), false);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.RelatedResults);
        }

        [TestMethod]
        public void CreateDrawdownHeatMapResults_DifferentialRun()
        {
            var stressPeriods = GetStressPeriods();

            _mapDrawdownData = CreateTestMapOutputData(stressPeriods).OrderByDescending(x => x.StressPeriod).ToList();

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetDrawdownMapData())
                .Returns(() => _mapDrawdownData);

            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateDrawdownHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), true);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void CreateDrawdownHeatMapResults_OutOfOrder()
        {
            var stressPeriods = GetStressPeriods();

            _mapDrawdownData = CreateTestMapOutputData(stressPeriods).OrderByDescending(x => x.StressPeriod).ToList();

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetDrawdownMapData())
                .Returns(() => _mapDrawdownData);

            var sut = CreateLocationMapSubEngine();
            Action action = () => sut.CreateDrawdownHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), false);

            action.Should().Throw<Exception>().WithMessage("Data is out of order.");
        }

        [TestMethod]
        public void CreateWaterLevelByZoneHeatMapResultsNonDifferential()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.OutputLocationZonesExists())
                .Returns(() => true);

            MapOutputLocationZones(_modflowFileAccessorMock, _locationPositionMap);

            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateWaterLevelByZoneHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), false, _outputLocationGeoJSONRaw);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.RelatedResults);
        }

        [TestMethod]
        public void CreateWaterLevelByZoneHeatMapResultsDifferential()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods, 50);
            _mapBaselineData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.GetBaselineMapData())
                .Returns(() => _mapBaselineData);

            _modflowFileAccessorMock.Arrange(a => a.OutputLocationZonesExists())
                .Returns(() => true);

            MapOutputLocationZones(_modflowFileAccessorMock, _locationPositionMap);

            MapFriendlyZoneNames(_modflowFileAccessorMock);

            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateWaterLevelByZoneHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), true, _outputLocationGeoJSONRaw);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.RelatedResults);
        }

        [TestMethod]
        public void CreateWaterLevelByZoneHeatMapResultsDifferentialNegative()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods, -50);
            _mapBaselineData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.GetBaselineMapData())
                .Returns(() => _mapBaselineData);

            _modflowFileAccessorMock.Arrange(a => a.OutputLocationZonesExists())
                .Returns(() => true);

            MapOutputLocationZones(_modflowFileAccessorMock, _locationPositionMap);

            MapFriendlyZoneNames(_modflowFileAccessorMock);

            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateWaterLevelByZoneHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), true, _outputLocationGeoJSONRaw);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.RelatedResults);
        }

        [TestMethod]
        public void CreateWaterLevelByZoneHeatMapResultsOutputLocationZonesExistsIsFalse()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.OutputLocationZonesExists())
                .Returns(() => false);

            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateWaterLevelByZoneHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), false, _outputLocationGeoJSONRaw);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void CreateWaterLevelByZoneHeatMapResultsOutputZoneDataIsNull()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.OutputLocationZonesExists())
                .Returns(() => false);

            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateWaterLevelByZoneHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), false, null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void CreateWaterLevelByZoneHeatMapResultsMapOutputDataHasGreaterStressPeriodsThanAreReported()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.OutputLocationZonesExists())
                .Returns(() => false);

            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateWaterLevelByZoneHeatMapResults(_modflowFileAccessorMock, new List<StressPeriod>{ GetStressPeriods().First()}, false, null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void CreateWaterLevelByZoneHeatMapResultsOutputLocationZonesShorterThanOutputLocationGeoJSON()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods);
            _mapBaselineData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.OutputLocationZonesExists())
                .Returns(() => true);

            MapOutputLocationZones(_modflowFileAccessorMock, _badLocationPositionMap);

            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateWaterLevelByZoneHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), true, _outputLocationGeoJSONRaw);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void CreateWaterLevelByZoneHeatMapResultsOutputLocationZonesLongerThanOutputLocationGeoJSON()
        {
            var stressPeriods = GetStressPeriods();

            _mapOutputData = CreateTestMapOutputData(stressPeriods);
            _mapBaselineData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.OutputLocationZonesExists())
                .Returns(() => true);

            MapOutputLocationZones(_modflowFileAccessorMock, _locationPositionMap);

            var sut = CreateLocationMapSubEngine();
            var result = sut.CreateWaterLevelByZoneHeatMapResults(_modflowFileAccessorMock, GetStressPeriods(), false, _badOutputLocationGeoJSONRaw);

            Assert.IsNull(result);
        }

        private void MapFriendlyZoneNames(IModelFileAccessor modflowFileAccessorMock)
        {
            _locationPositionMap.ForEach(x =>
            {
                modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(x))
                    .Returns(() => "Zone " + x);
            });
        }

        private void MapOutputLocationZones(IModelFileAccessor modflowFileAccessorMock, List<string> locationZonesToMap)
        {
            locationZonesToMap.ForEach(x =>
            {
                modflowFileAccessorMock.Arrange(a => a.GetOutputLocationZones(x))
                    .Returns(() => new List<string>{x});
            });
        }

        [TestMethod]
        public void INTERNAL_GetDifferentialLegendData()
        {
            var sut = CreateLocationMapSubEngine();

            string calculateColor(MapLocationState state, double value, double min, double max) => 
                LocationMapOutputSubEngine.GetDifferentialColor(state, value, min, max, false);

            var result = sut.GetDifferentialLegendData(new LocationMapOutputSubEngine.MapDataStats
            {
                HasRanDry = true,
                HasWasDry = true,
                HasIsDry = false,
                MinimumValue = 0,
                MaximumValue = 5996
            }, calculateColor);

            result.Should().HaveCount(13);
            result[0].Value.Should().Be(5996);
            result[0].IncreaseColor.Should().Be("#0000FF");
            result[0].DecreaseColor.Should().Be("#FF0000");
            result[1].Value.Should().Be(5996 * .9);
            result[1].IncreaseColor.Should().Be("#1313FF");
            result[1].DecreaseColor.Should().Be("#FF1313");
            result[2].Value.Should().Be(5996 * .8);
            result[2].IncreaseColor.Should().Be("#2828FF");
            result[2].DecreaseColor.Should().Be("#FF2828");
            result[3].Value.Should().Be(5996 * .7);
            result[3].IncreaseColor.Should().Be("#3C3CFF");
            result[3].DecreaseColor.Should().Be("#FF3C3C");
            result[4].Value.Should().Be(5996 * .6);
            result[4].IncreaseColor.Should().Be("#5151FF");
            result[4].DecreaseColor.Should().Be("#FF5151");
            result[5].Value.Should().Be(5996 * .5);
            result[5].IncreaseColor.Should().Be("#6666FF");
            result[5].DecreaseColor.Should().Be("#FF6666");
            result[6].Value.Should().Be(5996 * .4);
            result[6].IncreaseColor.Should().Be("#7979FF");
            result[6].DecreaseColor.Should().Be("#FF7979");
            result[7].Value.Should().Be(5996 * .3);
            result[7].IncreaseColor.Should().Be("#8E8EFF");
            result[7].DecreaseColor.Should().Be("#FF8E8E");
            result[8].Value.Should().Be(5996 * .2);
            result[8].IncreaseColor.Should().Be("#A2A2FF");
            result[8].DecreaseColor.Should().Be("#FFA2A2");
            result[9].Value.Should().Be(5996 * .1);
            result[9].IncreaseColor.Should().Be("#B7B7FF");
            result[9].DecreaseColor.Should().Be("#FFB7B7");
            result[10].Value.Should().Be(5996 * .01);
            result[10].IncreaseColor.Should().Be("#CCCCFF");
            result[10].DecreaseColor.Should().Be("#FFCCCC");
            result[11].Value.Should().Be(0);
            result[11].Text.Should().Be("Ran Dry");
            result[11].IncreaseColor.Should().Be("#000000");
            result[12].Value.Should().Be(0);
            result[12].Text.Should().Be("Was Dry");
            result[12].IncreaseColor.Should().Be("#00FF00");
        }

        [TestMethod]
        public void INTERNAL_GetDifferentialLegendData_ReverseColors()
        {
            var sut = CreateLocationMapSubEngine();

            string calculateColor(MapLocationState state, double value, double min, double max) =>
                LocationMapOutputSubEngine.GetDifferentialColor(state, value, min, max, true);

            var result = sut.GetDifferentialLegendData(new LocationMapOutputSubEngine.MapDataStats
            {
                HasRanDry = true,
                HasWasDry = true,
                HasIsDry = false,
                MinimumValue = 0,
                MaximumValue = 5996
            }, calculateColor);

            result.Should().HaveCount(13);
            result[0].Value.Should().Be(5996);
            result[0].IncreaseColor.Should().Be("#FF0000");
            result[0].DecreaseColor.Should().Be("#0000FF");
            result[1].Value.Should().Be(5996 * .9);
            result[1].IncreaseColor.Should().Be("#FF1313");
            result[1].DecreaseColor.Should().Be("#1313FF");
            result[2].Value.Should().Be(5996 * .8);
            result[2].IncreaseColor.Should().Be("#FF2828");
            result[2].DecreaseColor.Should().Be("#2828FF");
            result[3].Value.Should().Be(5996 * .7);
            result[3].IncreaseColor.Should().Be("#FF3C3C");
            result[3].DecreaseColor.Should().Be("#3C3CFF");
            result[4].Value.Should().Be(5996 * .6);
            result[4].IncreaseColor.Should().Be("#FF5151");
            result[4].DecreaseColor.Should().Be("#5151FF");
            result[5].Value.Should().Be(5996 * .5);
            result[5].IncreaseColor.Should().Be("#FF6666");
            result[5].DecreaseColor.Should().Be("#6666FF");
            result[6].Value.Should().Be(5996 * .4);
            result[6].IncreaseColor.Should().Be("#FF7979");
            result[6].DecreaseColor.Should().Be("#7979FF");
            result[7].Value.Should().Be(5996 * .3);
            result[7].IncreaseColor.Should().Be("#FF8E8E");
            result[7].DecreaseColor.Should().Be("#8E8EFF");
            result[8].Value.Should().Be(5996 * .2);
            result[8].IncreaseColor.Should().Be("#FFA2A2");
            result[8].DecreaseColor.Should().Be("#A2A2FF");
            result[9].Value.Should().Be(5996 * .1);
            result[9].IncreaseColor.Should().Be("#FFB7B7");
            result[9].DecreaseColor.Should().Be("#B7B7FF");
            result[10].Value.Should().Be(5996 * .01);
            result[10].IncreaseColor.Should().Be("#FFCCCC");
            result[10].DecreaseColor.Should().Be("#CCCCFF");
            result[11].Value.Should().Be(0);
            result[11].Text.Should().Be("Ran Dry");
            result[11].IncreaseColor.Should().Be("#000000");
            result[12].Value.Should().Be(0);
            result[12].Text.Should().Be("Was Dry");
            result[12].IncreaseColor.Should().Be("#00FF00");
        }

        [TestMethod]
        public void INTERNAL_GetLegendData()
        {
            var sut = CreateLocationMapSubEngine();

            string calculateColor(MapLocationState state, double value, double min, double max) =>
                LocationMapOutputSubEngine.GetNonDifferentialColor(state, value, min, max);

            var result = sut.GetLegendData(new LocationMapOutputSubEngine.MapDataStats
            {
                HasRanDry = false,
                HasWasDry = false,
                HasIsDry = true,
                MinimumValue = 3245,
                MaximumValue = 5996,
            }, calculateColor);

            // Range goes from 3245 to 5996 (so 2751 points in the range), we want equidistant breakdowns within the viable range

            result.Should().HaveCount(12);
            result[0].Value.Should().Be(3245 + 2751);
            result[0].IncreaseColor.Should().Be("#0000FF");
            result[1].Value.Should().Be(3245 + (2751 * .9));
            result[1].IncreaseColor.Should().Be("#1313FF");
            result[2].Value.Should().Be(3245 + (2751 * .8));
            result[2].IncreaseColor.Should().Be("#2828FF");
            result[3].Value.Should().Be(3245 + (2751 * .7));
            result[3].IncreaseColor.Should().Be("#3C3CFF");
            result[4].Value.Should().Be(3245 + (2751 * .6));
            result[4].IncreaseColor.Should().Be("#5151FF");
            result[5].Value.Should().Be(3245 + (2751 * .5));
            result[5].IncreaseColor.Should().Be("#6666FF");
            result[6].Value.Should().Be(3245 + (2751 * .4));
            result[6].IncreaseColor.Should().Be("#7979FF");
            result[7].Value.Should().Be(3245 + (2751 * .3));
            result[7].IncreaseColor.Should().Be("#8E8EFF");
            result[8].Value.Should().Be(3245 + (2751 * .2));
            result[8].IncreaseColor.Should().Be("#A2A2FF");
            result[9].Value.Should().Be(3245 + (2751 * .1));
            result[9].IncreaseColor.Should().Be("#B7B7FF");
            result[10].Value.Should().Be(3245 + (2751 * .01));
            result[10].IncreaseColor.Should().Be("#CCCCFF");
            result[11].Value.Should().Be(0);
            result[11].Text.Should().Be("Is Dry");
        }

    }
}
