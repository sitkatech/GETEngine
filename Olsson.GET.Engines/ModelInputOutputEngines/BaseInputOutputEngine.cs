using Newtonsoft.Json;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Shared;
using Olsson.GET.Common.Utilities;
using System.Text;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    public abstract class BaseInputOutputEngine : BaseEngine
    {
        protected static void WriteOutputFile(Run run, IBlobFileAccessor fileAccessor, RunResultDetails result)
        {
            WriteOutputFile(run, fileAccessor, result, false, result.RunResultName);
        }

        protected static void WriteOutputFile(Run run, IBlobFileAccessor fileAccessor, RunResultDetails result, bool hidden, string name)
        {
            result.Version = "1.0";
            var outputFilePathForRun = StorageLocations.OutputFilePathForRun(run.FileStorageLocator, $"{(hidden ? "!" : "")}{result.RunResultId.ToString().PadLeft(3, '0')}-{name}.json");
            var appSettingsBlobStorageModelDataFolder = ConfigurationHelper.AppSettings.BlobStorageModelDataFolder;
            var fileContent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
            fileAccessor.SaveFile(outputFilePathForRun, appSettingsBlobStorageModelDataFolder, fileContent).Wait();
        }

        protected static void WriteKmlFile(Run run, IBlobFileAccessor fileAccessor, RunResultDetails result, bool hidden, string name)
        {
            var kmlContent = Encoding.UTF8.GetBytes(result.ResultSets[0].MapData.KmlString);

            fileAccessor.SaveFile(StorageLocations.OutputFilePathForRun(run.FileStorageLocator, $"{(hidden ? "!" : "")}{result.RunResultId.ToString().PadLeft(3, '0')}-{name}.kml"),
                ConfigurationHelper.AppSettings.BlobStorageModelDataFolder,
                kmlContent,
                "application/vnd.google-earth.kml+xml"
                );
        }
    }
}
