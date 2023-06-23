using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Engines.ModelInputOutputEngines;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.Utilities;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using ModelExecutable = Olsson.GET.Common.DataContracts.Models.ModelExecutable;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;
using RunStatus = Olsson.GET.Accessors.EntityFramework.RunStatus;

namespace Olsson.GET.Tests.EngineTests.ModelInputOutputEngines
{
    [TestClass]
    public class ModelInputOutputEngineTests
    {
        private readonly IModelFileAccessor _modflowFileAccessorMock = Mock.Create<IModelFileAccessor>(Behavior.Strict);
        private readonly IBlobFileAccessor _fileAccessorMock = Mock.Create<IBlobFileAccessor>(Behavior.Strict);
        private readonly Model _model = new Model(){BaseflowTableProcessingConfigurationID = 1};

        [TestMethod]
        public void IdsWithHeatMapsGenerated()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            _calculateImpactToBaseflowResults.Add(new RunResultDetails { RunResultName = "I2BF" });
            _createWaterLevelHeatMapResults = (new RelatedResultDetails
            {
                RelatedResults = new List<RunResultDetails>
                {
                    new RunResultDetails
                    {
                        RunResultName = "HM1",
                        ResultSets = new List<RunResultSet>
                        {
                            new RunResultSet
                            {
                                MapData = new MapData
                                {
                                    MapPoints = "some map points",
                                    KmlString = "some kml string"
                                }
                            }
                        }
                    },
                    new RunResultDetails
                    {
                        RunResultName = "HM2",
                        ResultSets = new List<RunResultSet>
                        {
                            new RunResultSet
                            {
                                MapData = new MapData
                                {
                                    MapPoints = "some map points",
                                    KmlString = "some kml string"
                                }
                            }
                        }
                    }
                },
                SetName = "Heat Maps"
            }, null);
            _generateListFileOutputResults = (new List<RunResultDetails>
            {
                new RunResultDetails
                {
                    RunResultName = "List"
                }
            }, null);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/001-I2BF.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/003-Heat Maps.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/!003-HM1.kml", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>())).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/!004-HM2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/!004-HM2.kml", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>())).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/005-List.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/002-Points of Interest.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>())).Returns(Task.CompletedTask);

            _modflowFileAccessorMock.Arrange(mfa => mfa.GetPointsOfInterest()).Returns(new List<PointOfInterest>());
            _modflowFileAccessorMock.Arrange(mfa => mfa.FindLocationCell(Arg.IsAny<double>(), Arg.IsAny<double>())).Returns(new LocationWithBounds());
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetBaselineMapData()).Returns(new List<MapOutputData>());
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetOutputMapData()).Returns(new List<MapOutputData>());
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetObservedPointsOfInterest(true)).Returns(new List<ObservedPointOfInterest>());

            var sut = CreateModelInputOutputEngine();
            sut.GenerateOutputFiles(new Run { FileStorageLocator = "fakeLocator", ShouldCreateMaps = true, OutputVolumeUnitID = VolumeUnit.AcreFeet.VolumeUnitID, IsDifferential = true, Model = _model });

            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/001-I2BF.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/003-Heat Maps.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/!003-HM1.kml", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>()), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/!004-HM2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/!004-HM2.kml", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>()), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/005-List.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
        }

        [TestMethod]
        public void EmptyImpactToBaseflow()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            _createWaterLevelHeatMapResults = (new RelatedResultDetails
            {
                RelatedResults = new List<RunResultDetails>
                {
                    new RunResultDetails
                    {
                        RunResultName = "HM1",
                        ResultSets = new List<RunResultSet>
                        {
                            new RunResultSet
                            {
                                MapData = new MapData
                                {
                                    MapPoints = "some map points",
                                    KmlString = "some kml string"
                                }
                            }
                        }
                    },
                    new RunResultDetails
                    {
                        RunResultName = "HM2",
                        ResultSets = new List<RunResultSet>
                        {
                            new RunResultSet
                            {
                                 MapData = new MapData
                                {
                                    MapPoints = "some map points",
                                    KmlString = "some kml string"
                                }
                            }
                        }
                    }
                },
                SetName = "Heat Maps"
            }, null);
            _generateListFileOutputResults = (new List<RunResultDetails>
            {
                new RunResultDetails
                {
                    RunResultName = "List"
                }
            }, null);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/002-Heat Maps.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask); ;
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/!002-HM1.kml", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>())).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/!003-HM2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/!003-HM2.kml", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>())).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/004-List.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/001-Points of Interest.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>())).Returns(Task.CompletedTask);

            _modflowFileAccessorMock.Arrange(mfa => mfa.GetPointsOfInterest()).Returns(new List<PointOfInterest>());
            _modflowFileAccessorMock.Arrange(mfa => mfa.FindLocationCell(Arg.IsAny<double>(), Arg.IsAny<double>())).Returns(new LocationWithBounds());
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetBaselineMapData()).Returns(new List<MapOutputData>());
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetOutputMapData()).Returns(new List<MapOutputData>());
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetObservedPointsOfInterest(true)).Returns(new List<ObservedPointOfInterest>());

            var sut = CreateModelInputOutputEngine();
            sut.GenerateOutputFiles(new Run { FileStorageLocator = "fakeLocator", ShouldCreateMaps = true, OutputVolumeUnitID = VolumeUnit.AcreFeet.VolumeUnitID, IsDifferential = true, Model = _model});

            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/002-Heat Maps.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/!002-HM1.kml", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>()), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/!003-HM2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/!003-HM2.kml", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>()), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/004-List.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/001-Points of Interest.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
        }

        [TestMethod]
        public void IdsWithHeatMapsGeneratedButHaveException()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            _calculateImpactToBaseflowResults.Add(new RunResultDetails { RunResultName = "I2BF" });
            _createWaterLevelHeatMapResults = (new RelatedResultDetails
            {
                RelatedResults = new List<RunResultDetails>
                {
                    new RunResultDetails
                    {
                        RunResultName = "HM1",
                        ResultSets = new List<RunResultSet>
                        {
                            new RunResultSet
                            {
                                MapData = new MapData
                                {
                                    MapPoints = "some map points",
                                    KmlString = "some kml string"
                                }
                            }
                        }
                    },
                    new RunResultDetails
                    {
                        RunResultName = "HM2",
                        ResultSets = new List<RunResultSet>
                        {
                            new RunResultSet
                            {
                                MapData = new MapData
                                {
                                    MapPoints = "some map points",
                                    KmlString = "some kml string"
                                }
                            }
                        }
                    }
                },
                SetName = "Heat Maps"
            }, new OutputDataInvalidException("Error", RunStatus.HasDryCells.RunStatusID));
            _generateListFileOutputResults = (new List<RunResultDetails>
            {
                new RunResultDetails
                {
                    RunResultName = "List",
                    ResultSets = new List<RunResultSet>()
                }
            }, null);
            _createZoneBudgetOutputResultsResult.Add(new RelatedResultDetails
            {
                RelatedResults = new List<RunResultDetails>
                {
                    new RunResultDetails
                    {
                        RunResultName = "ZB1-1",
                        ResultSets = new List<RunResultSet>()
                    },
                    new RunResultDetails
                    {
                        RunResultName = "ZB1-2",
                        ResultSets = new List<RunResultSet>()
                    }
                },
                SetName = "ZoneBudget1"
            });
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/001-I2BF.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/003-Heat Maps.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/!003-HM1.kml", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>())).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/!004-HM2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/!004-HM2.kml", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>())).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/005-ZoneBudget1.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/!006-ZB1-2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/007-List.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/002-Points of Interest.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>())).Returns(Task.CompletedTask);

            _modflowFileAccessorMock.Arrange(mfa => mfa.GetPointsOfInterest()).Returns(new List<PointOfInterest>());
            _modflowFileAccessorMock.Arrange(mfa => mfa.FindLocationCell(Arg.IsAny<double>(), Arg.IsAny<double>())).Returns(new LocationWithBounds());
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetBaselineMapData()).Returns(new List<MapOutputData>());
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetOutputMapData()).Returns(new List<MapOutputData>());
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetObservedPointsOfInterest(true)).Returns(new List<ObservedPointOfInterest>());

            var sut = CreateModelInputOutputEngine();
            Assert.ThrowsException<OutputDataInvalidException>(() => sut.GenerateOutputFiles(new Run { FileStorageLocator = "fakeLocator", ShouldCreateMaps = true, OutputVolumeUnitID = VolumeUnit.AcreFeet.VolumeUnitID, IsDifferential = true, Model = _model }));

            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/001-I2BF.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/003-Heat Maps.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/!003-HM1.kml", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>()), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/!004-HM2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/!004-HM2.kml", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>()), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/005-ZoneBudget1.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/!006-ZB1-2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/007-List.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/002-Points of Interest.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
        }

        [TestMethod]
        public void GeneratesZoneBudget()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            _createZoneBudgetOutputResultsResult.Add(new RelatedResultDetails
            {
                RelatedResults = new List<RunResultDetails>
                {
                    new RunResultDetails
                    {
                        RunResultName = "ZB1-1",
                        ResultSets = new List<RunResultSet>()
                    },
                    new RunResultDetails
                    {
                        RunResultName = "ZB1-2",
                        ResultSets = new List<RunResultSet>()
                    }
                },
                SetName = "ZoneBudget1"
            });
            _createZoneBudgetOutputResultsResult.Add(new RelatedResultDetails
            {
                RelatedResults = new List<RunResultDetails>
                {
                    new RunResultDetails
                    {
                        RunResultName = "ZB2-1",
                        ResultSets = new List<RunResultSet>()
                    },
                    new RunResultDetails
                    {
                        RunResultName = "ZB2-2",
                        ResultSets = new List<RunResultSet>()
                    }
                },
                SetName = "ZoneBudget2"
            });
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/002-ZoneBudget1.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/!003-ZB1-2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/004-ZoneBudget2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/!005-ZB2-2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/001-Points of Interest.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), Arg.IsAny<string>())).Returns(Task.CompletedTask); ;

            _modflowFileAccessorMock.Arrange(mfa => mfa.GetPointsOfInterest()).Returns(new List<PointOfInterest>());
            _modflowFileAccessorMock.Arrange(mfa => mfa.FindLocationCell(Arg.IsAny<double>(), Arg.IsAny<double>())).Returns(new LocationWithBounds());
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetBaselineMapData()).Returns(new List<MapOutputData>());
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetOutputMapData()).Returns(new List<MapOutputData>());
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetObservedPointsOfInterest(true)).Returns(new List<ObservedPointOfInterest>());

            var sut = CreateModelInputOutputEngine();
            sut.GenerateOutputFiles(new Run { FileStorageLocator = "fakeLocator", ShouldCreateMaps = true, OutputVolumeUnitID = VolumeUnit.AcreFeet.VolumeUnitID, IsDifferential = true, Model = _model });

            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/002-ZoneBudget1.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/!003-ZB1-2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/004-ZoneBudget2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
            _fileAccessorMock.Assert(a => a.SaveFile("fakeLocator/outputs/!005-ZB2-2.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null), Occurs.Once());
        }

        [TestMethod]
        public void StressPeriodData_Null()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            _getStressPeriodDataResult = null;

            var sut = CreateModelInputOutputEngine();
            Assert.ThrowsException<Exception>(() => sut.GenerateOutputFiles(new Run { FileStorageLocator = "fakeLocator", ShouldCreateMaps = true, IsDifferential = true }));

            _fileAccessorMock.Assert(a => a.SaveFile(Arg.AnyString, Arg.AnyString, Arg.IsAny<byte[]>(), null), Occurs.Never());
        }

        private ILocationMapOutputSubEngine _locationMapOutputSubEngineMock = Mock.Create<ILocationMapOutputSubEngine>(Behavior.Strict);
        private IListFileOutputSubEngine _listFileOutputSubEngineMock = Mock.Create<IListFileOutputSubEngine>(Behavior.Strict);
        private IImpactToBaseflowFileOutputSubEngine _impactToBaseflowFileOutputSubEngineMock = Mock.Create<IImpactToBaseflowFileOutputSubEngine>(Behavior.Strict);
        private IZoneBudgetOutputSubEngine _zoneBudgetOutputSubEngineMock = Mock.Create<IZoneBudgetOutputSubEngine>(Behavior.Strict);
        private (RelatedResultDetails OutputResults, OutputDataInvalidException Exception) _createWaterLevelHeatMapResults = (null, null);
        private RelatedResultDetails _createWaterLevelByZoneHeatMapResults = null;
        private RelatedResultDetails _createDrawdownHeatMapResults = null;
        private (List<RunResultDetails> OutputResults, OutputDataInvalidException Exception) _generateListFileOutputResults = (null, null);
        private List<RunResultDetails> _calculateImpactToBaseflowResults = new List<RunResultDetails>();
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

        [TestMethod]
        public void ModpathModelInputOutputEngine_CreateInputFile()
        {
            var run = new Run()
            {
                RunID = 1,
                Model = new Model()
                {
                    ModelExecutables = new List<ModelExecutable>
                    {
                        new ModelExecutable
                        {
                            ExecutableName = "mp.exe",
                            Arguments = "sim.mpsim"
                        }
                    }
                },
                RunWellParticleInputs = new List<RunWellParticleInput>() { new RunWellParticleInput() { Lat = 2, Lng = 2, ParticleCount = 4 }, new RunWellParticleInput() { Lat = 3, Lng = 3, ParticleCount = 8 } }

            };
            var modpathEngine = CreateModpathInputOutputEngine();

            string locationFileContent = null;

            _modflowFileAccessorMock.Arrange(mfa => mfa.GetSettings()).Returns(new ModelSettings() { ParticleRadius = 1 });
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetModpathLocationFileName(Arg.AnyString)).Returns("location.loc");
            _modflowFileAccessorMock.Arrange(mfa => mfa.WriteLocationFile(Arg.AnyString, Arg.AnyString))
                .DoInstead((string file, string data) => { locationFileContent = data; });
            _modflowFileAccessorMock.Arrange(mfa => mfa.FindLocationCell(Arg.AnyDouble, Arg.AnyDouble)).Returns(new LocationWithBounds() { Location = "1 1 1", BoundCoordinates = new List<Coordinate>() { new Coordinate() { Lat = 0, Lng = 0 }, new Coordinate() { Lat = 10, Lng = 10 } } });

            modpathEngine.GenerateInputFiles(run);

            _modflowFileAccessorMock.AssertAll();

            //Count row
            locationFileContent.Should().Contain("12 0\r\n");

            //verify particle local x,y for center(C) at 2,2, 1 particle radius, 4 particle count            
            //4|
            //3|   x
            //2|x  C  x
            //1|   x
            //----------------------------------------------------
            //  1  2  3  4

            locationFileContent.Should().Contain("1 1 1 0.3000 0.2000 0.0000 0.0 0");
            locationFileContent.Should().Contain("1 1 1 0.2000 0.3000 0.0000 0.0 0");
            locationFileContent.Should().Contain("1 1 1 0.1000 0.2000 0.0000 0.0 0");
            locationFileContent.Should().Contain("1 1 1 0.2000 0.1000 0.0000 0.0 0");
        }

        [TestMethod]
        public void ModpathModelInputOutputEngine_CreateOutputFiles()
        {
            var run = new Run()
            {
                RunID = 1,
                Model = new Model()
                {
                    ModelExecutables = new List<ModelExecutable>
                    {
                        new ModelExecutable
                        {
                            ExecutableName = "mp.exe",
                            Arguments = "sim.mpsim"
                        }
                    }
                },
                RunWellParticleInputs = new List<RunWellParticleInput>() { new RunWellParticleInput() { Lat = 2, Lng = 2, ParticleCount = 4 }, new RunWellParticleInput() { Lat = 3, Lng = 3, ParticleCount = 8 } },
                User = new Common.DataContracts.Users.User() { Email = "test@test.com.ar" }

            };
            var modpathEngine = CreateModpathInputOutputEngine();

            string savedFile = null;
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetSettings()).Returns(new ModelSettings() { ParticleRadius = 1, RowCount = 100, ColumnCount = 100, ColorRanges = new ColorRange[] { new ColorRange() { Color = "red", Min = 0, Max = 200 } } });
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetModpathListFileName(Arg.AnyString)).Returns("file.lst");
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetModpathListFileContent(Arg.AnyString)).Returns("blah blah");
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetModpathTimeSeriesFileName(Arg.AnyString)).Returns("file.name");
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetModpathTimeSeriesResult(Arg.AnyString)).Returns(new List<ModpathTimeSeries>() { new ModpathTimeSeries() { Layer = 1, LocalX = 0.5, LocalY = 0.5, CellNumber = 4000 } });
            _modflowFileAccessorMock.Arrange(mfa => mfa.FindCellBounds(Arg.AnyString)).Returns(new List<Coordinate>() { new Coordinate() { Lat = 0, Lng = 0 }, new Coordinate() { Lat = 10, Lng = 10 } });
            _fileAccessorMock
                .Arrange(fa => fa.SaveFile(Arg.AnyString, Arg.AnyString, Arg.IsAny<byte[]>(), null))
                .DoInstead((string path, string loc, byte[] data) =>
                {
                    savedFile = Encoding.Default.GetString(data);
                }).Returns(Task.CompletedTask);

            modpathEngine.GenerateOutputFiles(run);

            savedFile.Should().Contain("<coordinates>5,5,0</coordinates>");
        }

        [TestMethod]
        public void ModpathModelInputOutputEngine_CreateInputFile_CenterdIfCountIsOne()
        {
            var run = new Run()
            {
                RunID = 1,
                Model = new Model()
                {
                    ModelExecutables = new List<ModelExecutable>
                    {
                        new ModelExecutable
                        {
                            ExecutableName = "mp.exe",
                            Arguments = "sim.mpsim"
                        }
                    }
                },
                RunWellParticleInputs = new List<RunWellParticleInput>() { new RunWellParticleInput() { Lat = 2, Lng = 2, ParticleCount = 1 }, new RunWellParticleInput() { Lat = 3, Lng = 3, ParticleCount = 1 } }

            };
            var modpathEngine = CreateModpathInputOutputEngine();

            string locationFileContent = null;

            _modflowFileAccessorMock.Arrange(mfa => mfa.GetSettings()).Returns(new ModelSettings() { ParticleRadius = 1 });
            _modflowFileAccessorMock.Arrange(mfa => mfa.GetModpathLocationFileName(Arg.AnyString)).Returns("location.loc");
            _modflowFileAccessorMock.Arrange(mfa => mfa.WriteLocationFile(Arg.AnyString, Arg.AnyString))
                .DoInstead((string file, string data) => { locationFileContent = data; });
            _modflowFileAccessorMock.Arrange(mfa => mfa.FindLocationCell(Arg.AnyDouble, Arg.AnyDouble)).Returns(new LocationWithBounds() { Location = "1 1 1", BoundCoordinates = new List<Coordinate>() { new Coordinate() { Lat = 0, Lng = 0 }, new Coordinate() { Lat = 10, Lng = 10 } } });

            modpathEngine.GenerateInputFiles(run);

            _modflowFileAccessorMock.AssertAll();

            //Count row
            locationFileContent.Should().Contain("2 0\r\n");

            //centered
            locationFileContent.Should().Contain("1 1 1 0.2000 0.2000 0.0000 0.0 0");
            locationFileContent.Should().Contain("1 1 1 0.3000 0.3000 0.0000 0.0 0");
        }

        private List<RelatedResultDetails> _createZoneBudgetOutputResultsResult = new List<RelatedResultDetails>();

        private ModflowModelInputOutputEngine CreateModelInputOutputEngine()
        {
            var modflowFileAccessorFactory = Mock.Create<IModelFileAccessorFactory>();
            modflowFileAccessorFactory.Arrange(a => a.CreateModflowFileAccessor(Arg.IsAny<Model>()))
                .Returns(_modflowFileAccessorMock);

            _locationMapOutputSubEngineMock.Arrange(a => a.CreateWaterLevelHeatMapResults(Arg.IsAny<IModelFileAccessor>(), Arg.IsAny<List<StressPeriod>>(), Arg.IsAny<bool>()))
                .Returns(() => _createWaterLevelHeatMapResults);

            _locationMapOutputSubEngineMock.Arrange(a => a.CreateWaterLevelByZoneHeatMapResults(Arg.IsAny<IModelFileAccessor>(), Arg.IsAny<List<StressPeriod>>(), Arg.IsAny<bool>(), Arg.AnyString))
                .Returns(() => _createWaterLevelByZoneHeatMapResults);

            _locationMapOutputSubEngineMock.Arrange(a => a.CreateDrawdownHeatMapResults(Arg.IsAny<IModelFileAccessor>(), Arg.IsAny<List<StressPeriod>>(), Arg.IsAny<bool>()))
                .Returns(() => _createDrawdownHeatMapResults);

            _listFileOutputSubEngineMock.Arrange(a => a.GenerateListFileOutput(Arg.IsAny<IModelFileAccessor>(), Arg.IsAny<List<StressPeriod>>(), VolumeUnit.AcreFeet.VolumeUnitID, true))
                .Returns(() => _generateListFileOutputResults);

            _impactToBaseflowFileOutputSubEngineMock.Arrange(a => a.CalculateImpactToBaseflow(Arg.IsAny<IModelFileAccessor>(), Arg.IsAny<List<StressPeriod>>(), VolumeUnit.AcreFeet.VolumeUnitID, true))
                .Returns(() => _calculateImpactToBaseflowResults);

            _zoneBudgetOutputSubEngineMock.Arrange(a => a.CreateZoneBudgetOutputResults(Arg.IsAny<IModelFileAccessor>(), Arg.IsAny<List<StressPeriod>>(), VolumeUnit.AcreFeet.VolumeUnitID, true))
                .Returns(() => _createZoneBudgetOutputResultsResult);

            _modflowFileAccessorMock.Arrange(a => a.GetStressPeriodData())
                .Returns(() => _getStressPeriodDataResult);

            var sut = new ModflowModelInputOutputEngine(_model);
            sut.AccessorFactory = new AccessorFactory();
            sut.AccessorFactory.AddOverride(modflowFileAccessorFactory);
            sut.AccessorFactory.AddOverride(_fileAccessorMock);
            sut.LocationMapOutputSubEngine = _locationMapOutputSubEngineMock;
            sut.ListFileOutputSubEngine = _listFileOutputSubEngineMock;
            sut.ZoneBudgetOutputSubEngine = _zoneBudgetOutputSubEngineMock;
            sut.ImpactToBaseflowFileOutputSubEngine = _impactToBaseflowFileOutputSubEngineMock;
            return sut;
        }

        private ModpathModelInputOutputEngine CreateModpathInputOutputEngine()
        {
            var modflowFileAccessorFactory = Mock.Create<IModelFileAccessorFactory>();
            modflowFileAccessorFactory.Arrange(a => a.CreateModflowFileAccessor(Arg.IsAny<Model>()))
                .Returns(_modflowFileAccessorMock);

            var sut = new ModpathModelInputOutputEngine();
            sut.AccessorFactory = new AccessorFactory();
            sut.AccessorFactory.AddOverride(modflowFileAccessorFactory);
            sut.AccessorFactory.AddOverride(_fileAccessorMock);

            return sut;
        }
    }
}
