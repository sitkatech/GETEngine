using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    //These are tests to help with development of output processing.  They should not be used as part of a normal test run.
    [TestClass]
    public class TestOutputData
    {
        //private Model _model;

        //private readonly IFileAccessor _fileAccessorMock = Mock.Create<IFileAccessor>(Behavior.Strict);

        //private ModelInputOutputEngine CreateCentralPlatteModelInputOutputEngine()
        //{
        //    var sut = new ModelInputOutputEngine(_model);
        //    sut.AccessorFactory = new AccessorFactory();
        //    sut.AccessorFactory.AddOverride(_fileAccessorMock);
        //    return sut;
        //}

        //[TestMethod]
        //public void Test()
        //{
        //    ConfigurationHelper.AppSettings.ModflowDataFolder = @"C:\Temp\modeldump";
        //    ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";

        //    _model = new ModelAccessor().FindAllModels().First(a => a.Id == 2);

        //    _fileAccessorMock.Arrange(a => a.SaveFile(Arg.AnyString, Arg.AnyString, Arg.IsAny<byte[]>()))
        //        .DoInstead<string, string, byte[]>((a, b, c) =>
        //        {
        //            var filePath = $@"c:\temp\modeldumpoutput\{b}\{a}";
        //            var directoryPath = System.IO.Path.GetDirectoryName(filePath);
        //            if (!Directory.Exists(directoryPath))
        //            {
        //                Directory.CreateDirectory(directoryPath);
        //            }
        //            System.IO.File.WriteAllBytes(filePath, c);
        //        });

        //    SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
        //    SqlProviderServices.SqlServerTypesAssemblyName = "Microsoft.SqlServer.Types, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

        //    var sut = CreateCentralPlatteModelInputOutputEngine();
        //    sut.GenerateOutputFiles(new Run { FileStorageLocator = "fakeLocator", Id = 48, ShouldCreateMaps = true });

        //    Assert.Inconclusive("This test should only be used for testing of static output data.  It should be commented out as part of normal test runs.");
        //}

        //[TestMethod]
        //public void Test_cpnrd()
        //{
        //    ConfigurationHelper.AppSettings.ModflowDataFolder = @"C:\Temp\dockermodeldump_cpnrd";
        //    ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";

        //    _model.MapRunFileName = "CPNRD.hds";

        //    _fileAccessorMock.Arrange(a => a.SaveFile(Arg.AnyString, Arg.AnyString, Arg.IsAny<byte[]>()))
        //        .DoInstead<string, string, byte[]>((a, b, c) =>
        //        {
        //            var filePath = $@"c:\temp\dockerOutput_cpnrd\{b}\{a}";
        //            var directoryPath = System.IO.Path.GetDirectoryName(filePath);
        //            if (!Directory.Exists(directoryPath))
        //            {
        //                Directory.CreateDirectory(directoryPath);
        //            }
        //            System.IO.File.WriteAllBytes(filePath, c);
        //        });

        //    SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
        //    SqlProviderServices.SqlServerTypesAssemblyName = "Microsoft.SqlServer.Types, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

        //    var sut = CreateCentralPlatteModelInputOutputEngine();
        //    sut.GenerateOutputFiles(new Run { FileStorageLocator = "fakeLocator", Id = 48, ShouldCreateMaps = true });

        //    Assert.Inconclusive("This test should only be used for testing of static output data.  It should be commented out as part of normal test runs.");
        //}

        //[TestMethod]
        //public void Test_cohyst()
        //{
        //    ConfigurationHelper.AppSettings.ModflowDataFolder = @"C:\Temp\dockermodeldump_cohyst";
        //    ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";

        //    _model.NamFileName = "COHYST2010_28b_14_28.nam";
        //    _model.RunFileName = "COHYST2010_28b_14_28_sfr.out";

        //    _fileAccessorMock.Arrange(a => a.SaveFile(Arg.AnyString, Arg.AnyString, Arg.IsAny<byte[]>()))
        //        .DoInstead<string, string, byte[]>((a, b, c) =>
        //        {
        //            var filePath = $@"c:\temp\dockerOutput_cohyst\{b}\{a}";
        //            var directoryPath = System.IO.Path.GetDirectoryName(filePath);
        //            if (!Directory.Exists(directoryPath))
        //            {
        //                Directory.CreateDirectory(directoryPath);
        //            }
        //            System.IO.File.WriteAllBytes(filePath, c);
        //        });
        //    var sut = CreateCentralPlatteModelInputOutputEngine();
        //    sut.GenerateOutputFiles(new Run { FileStorageLocator = "fakeLocator" });

        //    Assert.Inconclusive("This test should only be used for testing of static output data.  It should be commented out as part of normal test runs.");
        //}
    }
}