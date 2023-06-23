using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Engines.ModelInputOutputEngines;
using Telerik.JustMock;
using System.Configuration;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using Telerik.JustMock.Helpers;
using Olsson.GET.Tests.EngineTests;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.Utilities;
using SqlServerTypes;
using BaseflowTableProcessingConfiguration = Olsson.GET.Common.DataContracts.Models.BaseflowTableProcessingConfiguration;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using ModelExecutable = Olsson.GET.Common.DataContracts.Models.ModelExecutable;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;

namespace IntegrationTests
{
    [TestClass]
    public class CentralPlatteModflowTests
    {
        private readonly IBlobFileAccessor _fileAccessorMock = Mock.Create<IBlobFileAccessor>(Behavior.Strict);

        private Model _model = new Model
        {
            ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
            ModelGridTypeID = (int)ModelGridTypeEnum.Unstructured,
            StartDateTime = new DateTime(2011, 1, 1),
            RunFileName = "CPNRD_streamflow.dat",
            MapRunFileName = "CPNRD.hds",
            AllowablePercentDiscrepancy = 1,
            BaseflowTableProcessingConfigurationID = 1,
            BaseflowTableProcessingConfiguration = new BaseflowTableProcessingConfiguration()
            {
                BaseflowTableProcessingConfigurationID = 1,
                BaseflowTableIndicatorRegexPattern = @"^\s+STREAM LISTING\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$",
                SegmentColumnNum = 2,
                FlowToAquiferColumnNum = 5,
                ReachColumnNum = 3
            },
            ModelExecutables = new List<ModelExecutable>
            {
                new ModelExecutable
                {
                    Arguments = "CPNRD.nam"
                }
            }
    };

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void InitTypes()
        {
            Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
            // todo: assembly?
            //SqlProviderServices.SqlServerTypesAssemblyName = "Microsoft.SqlServer.Types, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";
        }

        [TestMethod]
        public void GeneratesOutputFiles()
        {
            ConfigurationHelper.AppSettings.ModflowDataFolder = @"ModflowTestFiles\CentralPlatteNaturalResourceDistrictSample";
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            RunResultDetails totalResult = null;
            RunResultDetails pointsOfInterestResult = null;
            RunResultDetails listFileResult = null;
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/001-Impacts to Baseflow.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null))
                .DoInstead<string, string, byte[]>((a, b, c) => totalResult = JsonConvert.DeserializeObject<RunResultDetails>(System.Text.Encoding.UTF8.GetString(c)));
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/002-Points of Interest.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null))
                .DoInstead<string, string, byte[]>((a, b, c) => pointsOfInterestResult = JsonConvert.DeserializeObject<RunResultDetails>(System.Text.Encoding.UTF8.GetString(c)));
            _fileAccessorMock.Arrange(a => a.SaveFile("fakeLocator/outputs/003-List File Output.json", "fakeModelDataFolder", Arg.IsAny<byte[]>(), null))
                .DoInstead<string, string, byte[]>((a, b, c) =>
                {
                    listFileResult = JsonConvert.DeserializeObject<RunResultDetails>(System.Text.Encoding.UTF8.GetString(c));
                });
            var sut = CreateCentralPlatteModelInputOutputEngine();
            sut.GenerateOutputFiles(new Run { FileStorageLocator = "fakeLocator", OutputVolumeUnitID = VolumeUnit.AcreFeet.VolumeUnitID, IsDifferential = true, Model = _model });

            var expectedResultData = GetExpectedResultData().ToList();
            TestImpactToBaseflowResult(totalResult, expectedResultData, "Impacts to Baseflow", i => expectedResultData[i].Total, 1);
            TestPointsOfInterestResult(pointsOfInterestResult, "Points of Interest", 2);
            TestListFileResult(listFileResult, 3);
        }

        private static void TestImpactToBaseflowResult(RunResultDetails totalResult, List<ExpectedBaseFlowResultData> expectedResultData, string expectedTitle, Func<int, double> expectedValueFunc, int expectedRunResultId)
        {
            totalResult.Should().NotBeNull();
            totalResult.RunResultId.Should().Be(expectedRunResultId);
            totalResult.RunResultName.Should().Be(expectedTitle);
            totalResult.ResultSets.Should().NotBeNull().And.Subject.Count().Should().Be(2);
            totalResult.ResultSets[0].Name.Should().Be("Rate");
            totalResult.ResultSets[0].DataType.Should().Be("Acre-Feet");
            totalResult.ResultSets[0].DisplayType.Should().Be(RunResultDisplayType.LineChart);
            totalResult.ResultSets[0].DataSeries.Should().NotBeNull().And.Subject.Count().Should().Be(4);
            totalResult.ResultSets[0].DataSeries[0].DataPoints.Should().NotBeNull().And.Subject.Count().Should().Be(expectedResultData.Count);
            for (var i = 0; i < expectedResultData.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine(totalResult.ResultSets[0].DataSeries[0].DataPoints[i].Value);
                var date = new DateTime(expectedResultData[i].Year, expectedResultData[i].Month, 1);
                if (date.Year % 2 != 0 || date.Month != 2)
                {
                    totalResult.ResultSets[0].DataSeries[0].DataPoints[i].Date.Should().Be(date);
                    TestUtilities.AssertAreEqualWithCalculatedDelta(expectedValueFunc(i), totalResult.ResultSets[0].DataSeries[0].DataPoints[i].Value);
                }
            }
            totalResult.ResultSets[0].TextDisplay.Should().BeNull();
        }

        private static void TestPointsOfInterestResult(RunResultDetails pointsOfInterestResult, string expectedTitle, int expectedRunResultId)
        {
            pointsOfInterestResult.Should().NotBeNull();
            pointsOfInterestResult.RunResultId.Should().Be(expectedRunResultId);
            pointsOfInterestResult.RunResultName.Should().Be(expectedTitle);
            pointsOfInterestResult.ResultSets.Should().NotBeNull().And.Subject.Count().Should().Be(1);
            pointsOfInterestResult.ResultSets[0].Name.Should().Be("Points of Interest");
            pointsOfInterestResult.ResultSets[0].DataType.Should().Be("Elevation (feet)");
            pointsOfInterestResult.ResultSets[0].DisplayType.Should().Be(RunResultDisplayType.LineChart);
            pointsOfInterestResult.ResultSets[0].DataSeries.Should().NotBeNull().And.Subject.Count().Should().Be(2);
            pointsOfInterestResult.ResultSets[0].TextDisplay.Should().BeNull();
        }

        private static void TestListFileResult(RunResultDetails totalResult, int expectedRunResultId)
        {
            totalResult.Should().NotBeNull();
            totalResult.RunResultId.Should().Be(expectedRunResultId);
            totalResult.RunResultName.Should().Be("List File Output");
            totalResult.ResultSets.Should().NotBeNull().And.Subject.Count().Should().Be(1);
            totalResult.ResultSets[0].Name.Should().Be("List File Output");
            totalResult.ResultSets[0].DisplayType.Should().Be(RunResultDisplayType.Text);
            totalResult.ResultSets[0].DataSeries.Should().BeNull();
            totalResult.ResultSets[0].TextDisplay.Should().NotBeNull();
            totalResult.ResultSets[0].TextDisplay.FileName.Should().Be("ListFile.txt");
            totalResult.ResultSets[0].TextDisplay.Text.Should().Be(System.IO.File.ReadAllText("ModflowTestFiles/CentralPlatteNaturalResourceDistrictSample/CPNRD.lst"));
        }

        private IEnumerable<ExpectedBaseFlowResultData> GetExpectedResultData()
        {
            var expectedResultData = new CsvHelper.CsvReader(System.IO.File.OpenText(@"ModflowTestFiles\CentralPlatteNaturalResourceDistrictSample\OlssonPostProcess\MonthlyValues.csv"));
            expectedResultData.Read();
            expectedResultData.ReadHeader();
            while (expectedResultData.Read())
            {
                yield return new ExpectedBaseFlowResultData
                {
                    Year = expectedResultData.GetField<int>("Year"),
                    Month = expectedResultData.GetField<int>("Month"),
                    Zone1 = expectedResultData.GetField<double>("1"),
                    Zone2 = expectedResultData.GetField<double>("2"),
                    Zone3 = expectedResultData.GetField<double>("3"),
                    Total = expectedResultData.GetField<double>("Total")
                };
            }
        }

        private class ExpectedBaseFlowResultData
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public double Zone1 { get; set; }
            public double Zone2 { get; set; }
            public double Zone3 { get; set; }
            public double Total { get; set; }
        }
        private ModflowModelInputOutputEngine CreateCentralPlatteModelInputOutputEngine()
        {
            var sut = new ModflowModelInputOutputEngine(_model);
            sut.AccessorFactory = new AccessorFactory();
            sut.AccessorFactory.AddOverride(_fileAccessorMock);
            return sut;
        }
    }
}
