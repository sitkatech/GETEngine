using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Models;
using Telerik.JustMock;
using Olsson.GET.Engines.ModelInputOutputEngines;
using Telerik.JustMock.Helpers;
using Olsson.GET.Common.DataContracts.Runs;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace Olsson.GET.Tests.EngineTests.ModelInputOutputEngines
{
    [TestClass]
    public class PointsOfInterestSubEngineTests
    {
        private readonly IModelFileAccessor _modflowFileAccessorMock = Mock.Create<IModelFileAccessor>(Behavior.Strict);
        private readonly IBlobFileAccessor _fileAccessorMock = Mock.Create<IBlobFileAccessor>(Behavior.Strict);
        private List<string> _locationPositionMap;
        private List<MapOutputData> _mapOutputData;
        private List<MapOutputData> _mapBaselineData;
        private List<ObservedPointOfInterest> _observedPointsOfInterest;

        private readonly Model _model = new Model
        {
            StartDateTime = new DateTime(2011, 1, 1)
        };

        private List<StressPeriod> GetStressPeriods()
        {
            return new List<StressPeriod>
            {
                new StressPeriod{Days = 31, NumberOfTimeSteps = 2},
                new StressPeriod{Days = 28, NumberOfTimeSteps = 2}
            };
        }

        private PointsOfInterestOutputSubEngine CreatePointsOfInterestSubEngine()
        {
            var modflowFileAccessorFactory = Mock.Create<IModelFileAccessorFactory>();
            modflowFileAccessorFactory.Arrange(a => a.CreateModflowFileAccessor(Arg.IsAny<Model>()))
                .Returns(_modflowFileAccessorMock);

            var sut = new PointsOfInterestOutputSubEngine(_model);
            return sut;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _locationPositionMap = new List<string> { "a", "b", "c", "d" };
            var stressPeriods = GetStressPeriods();
            _model.NumberOfStressPeriods = stressPeriods.Count;

            _mapBaselineData = CreateTestMapOutputData(stressPeriods);
            _mapOutputData = CreateTestMapOutputData(stressPeriods);

            _modflowFileAccessorMock.Arrange(a => a.GetLocationPositionMap())
                .Returns(() => _locationPositionMap);

            _modflowFileAccessorMock.Arrange(a => a.GetOutputMapData())
                .Returns(() => _mapOutputData);

            _modflowFileAccessorMock.Arrange(a => a.GetBaselineMapData())
                .Returns(() => _mapBaselineData);

            _modflowFileAccessorMock.Arrange(a => a.GetObservedPointsOfInterest(Arg.IsAny<bool>()))
                .Returns(() => _observedPointsOfInterest);
        }

        private List<MapOutputData> CreateTestMapOutputData(List<StressPeriod> stressPeriods)
        {
            var result = new List<MapOutputData>();
            foreach (var location in _locationPositionMap)
            {
                for (var j = 0; j < stressPeriods.Count; j++)
                {
                    for (var i = 0; i < stressPeriods[j].NumberOfTimeSteps; i++)
                    {
                        result.Add(new MapOutputData
                        {
                            Location = location,
                            StressPeriod = j + 1,
                            TimeStep = i + 1,
                            Value = j | i
                        });
                    }
                }
            }
            return result;
        }

        [TestMethod]
        public void PointsOfInterest_NoPOI()
        {

            _modflowFileAccessorMock.Arrange(mfa => mfa.GetPointsOfInterest()).Returns<LocationWithBounds>(null);
            var resultId = 0;
            var sut = CreatePointsOfInterestSubEngine();
            var result = sut.GeneratePointsOfInterestGraphOutput(_modflowFileAccessorMock, GetStressPeriods(), resultId, true);

            result.Should().NotBeNull();
            result.Should().HaveCount(0);

        }

        [TestMethod]
        public void PointsOfInterest_OnePOI()
        {           
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetPointsOfInterest()).Returns(new List<PointOfInterest>() { 
                new PointOfInterest()
                {
                    Coordinate = new Coordinate(){ Lat = 0.0, Lng = 0.0},
                    Name = "a"
                }
            });
            _modflowFileAccessorMock.Arrange(mfa => mfa.FindLocationCell(Arg.IsAny<double>(), Arg.IsAny<double>())).Returns(new LocationWithBounds() {
                Location = "a",
                BoundCoordinates = new List<Coordinate>() { 
                    new Coordinate() { 
                        Lat = 0.0,
                        Lng = 0.0
                    }
                }
            });
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetBaselineMapData()).Returns(_mapBaselineData);
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetOutputMapData()).Returns(_mapOutputData);
            var resultId = 0;
            var sut = CreatePointsOfInterestSubEngine();
            var result = sut.GeneratePointsOfInterestGraphOutput(_modflowFileAccessorMock, GetStressPeriods(), resultId, true);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Select(x => x.ResultSets));
            Assert.IsNotNull(result.Select(x => x.ResultSets.Select(y => y.DataSeries)));

            Assert.AreEqual(result[0].ResultSets[0].DataType, "Elevation (feet)");
            Assert.IsTrue(result[0].ResultSets[0].DataSeries[0].DataPoints.Count() > 0);
        }

        [TestMethod]
        public void PointsOfInterest_TwoPOI()
        {
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetPointsOfInterest()).Returns(new List<PointOfInterest>() {
                new PointOfInterest()
                {
                    Coordinate = new Coordinate(){ Lat = 0.0, Lng = 0.0},
                    Name = "a"
                },
                new PointOfInterest()
                {
                    Coordinate = new Coordinate(){ Lat = 1.0, Lng = 1.0},
                    Name = "b"
                }
            });

            _modflowFileAccessorMock.Arrange(mfa => mfa.FindLocationCell(0.0, 0.0)).Returns(new LocationWithBounds()
            {
                Location = "a",
                BoundCoordinates = new List<Coordinate>() {
                    new Coordinate() {
                        Lat = 0.0,
                        Lng = 0.0
                    }
                }
            });

            _modflowFileAccessorMock.Arrange(mfa => mfa.FindLocationCell(1.0, 1.0)).Returns(new LocationWithBounds()
            {
                Location = "b",
                BoundCoordinates = new List<Coordinate>() {
                    new Coordinate() {
                        Lat = 1.0,
                        Lng = 1.0
                    }
                }
            });

            _modflowFileAccessorMock.Arrange(mfa => mfa.GetBaselineMapData()).Returns(_mapBaselineData);
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetOutputMapData()).Returns(_mapOutputData);
            var resultId = 0;
            var sut = CreatePointsOfInterestSubEngine();
            var result = sut.GeneratePointsOfInterestGraphOutput(_modflowFileAccessorMock, GetStressPeriods(), resultId, true);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Select(x => x.ResultSets));
            Assert.IsNotNull(result.Select(x => x.ResultSets.Select(y => y.DataSeries)));

            Assert.AreEqual(result[0].ResultSets[0].DataType, "Elevation (feet)");
            
            Assert.IsTrue(result[0].ResultSets[0].DataSeries.Count() == 2);
            Assert.IsTrue(result[0].ResultSets[0].DataSeries[0].DataPoints.Count() > 0);
            Assert.IsTrue(result[0].ResultSets[0].DataSeries[1].DataPoints.Count() > 0);
        }

        [TestMethod]
        public void PointsOfInterest_NonDifferential()
        {
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetPointsOfInterest()).Returns(new List<PointOfInterest>() {
                new PointOfInterest()
                {
                    Coordinate = new Coordinate(){ Lat = 0.0, Lng = 0.0},
                    Name = "a"
                }
            });
            _modflowFileAccessorMock.Arrange(mfa => mfa.FindLocationCell(Arg.IsAny<double>(), Arg.IsAny<double>())).Returns(new LocationWithBounds()
            {
                Location = "a",
                BoundCoordinates = new List<Coordinate>() {
                    new Coordinate() {
                        Lat = 0.0,
                        Lng = 0.0
                    }
                }
            });
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetOutputMapData()).Returns(_mapOutputData);
            var resultId = 0;
            var sut = CreatePointsOfInterestSubEngine();
            var result = sut.GeneratePointsOfInterestGraphOutput(_modflowFileAccessorMock, GetStressPeriods(), resultId, false);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Select(x => x.ResultSets));
            Assert.IsNotNull(result.Select(x => x.ResultSets.Select(y => y.DataSeries)));

            Assert.AreEqual(result[0].ResultSets[0].DataType, "Elevation (feet)");
            Assert.IsTrue(result[0].ResultSets[0].DataSeries[0].DataPoints.Count() > 0);
        }

        [TestMethod]
        public void PointsOfInterest_Observed()
        {
            _observedPointsOfInterest = new List<ObservedPointOfInterest>
            {
                new ObservedPointOfInterest
                {
                    LocationSeriesName = "a - Observed",
                    Period = 1,
                    ValueInCubicFeet = -99,
                }
            };

            _modflowFileAccessorMock.Arrange(mfa => mfa.GetPointsOfInterest()).Returns(new List<PointOfInterest>() {
                new PointOfInterest()
                {
                    Coordinate = new Coordinate(){ Lat = 0.0, Lng = 0.0},
                    Name = "a"
                }
            });
            _modflowFileAccessorMock.Arrange(mfa => mfa.FindLocationCell(Arg.IsAny<double>(), Arg.IsAny<double>())).Returns(new LocationWithBounds()
            {
                Location = "a",
                BoundCoordinates = new List<Coordinate>() {
                    new Coordinate() {
                        Lat = 0.0,
                        Lng = 0.0
                    }
                }
            });
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetBaselineMapData()).Returns(_mapBaselineData);
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetOutputMapData()).Returns(_mapOutputData);
            var resultId = 0;
            var sut = CreatePointsOfInterestSubEngine();
            var result = sut.GeneratePointsOfInterestGraphOutput(_modflowFileAccessorMock, GetStressPeriods(), resultId, true);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Select(x => x.ResultSets));
            Assert.IsNotNull(result.Select(x => x.ResultSets.Select(y => y.DataSeries)));

            Assert.AreEqual(result[0].ResultSets[0].DataType, "Elevation (feet)");
            Assert.IsTrue(result[0].ResultSets[0].DataSeries[0].DataPoints.Count() > 0);
        }
    }
}
