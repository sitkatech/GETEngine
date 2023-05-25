namespace Olsson.GET.Common.Shared
{
    public static class StorageLocations
    {
        private const string PARSED_INPUT_FILE_NAME = "inputs.json";
        private const string PARSED_WELL_INPUT_FILE_NAME = "mapinputs.json";
        private const string PARSED_WELL_PARTICLE_INPUT_FILE_NAME = "mapparticleinputs.json";
        private const string PARSED_ZONE_INPUT_FILE_NAME = "mapzoneinputs.json";
        private const string INPUT_FOLDER_NAME = "inputs";
        private const string OUTPUT_FOLDER_NAME = "outputs";
        private const string GENERATE_INPUT_OUTPUT_FOLDER_NAME = "generateinputoutputs";
        private const string ANALYSIS_OUTPUT_FOLDER_NAME = "analysisoutputs";

        public static string InputFolderPathForRun(string fileStorageLocator)
        {
            return $"{fileStorageLocator}/{INPUT_FOLDER_NAME}";
        }
        public static string InputFilePathForRun(string fileStorageLocator, string inputFileName)
        {
            return $"{InputFolderPathForRun(fileStorageLocator)}/{inputFileName}";
        }
        public static string ParsedInputFilePathForRun(string fileStorageLocator)
        {
            return $"{InputFolderPathForRun(fileStorageLocator)}/{PARSED_INPUT_FILE_NAME}";
        }
        public static string ParsedWellInputFilePathForRun(string fileStorageLocator)
        {
            return $"{InputFolderPathForRun(fileStorageLocator)}/{PARSED_WELL_INPUT_FILE_NAME}";
        }
        public static string ParsedWellParticleInputFilePathForRun(string fileStorageLocator)
        {
            return $"{InputFolderPathForRun(fileStorageLocator)}/{PARSED_WELL_PARTICLE_INPUT_FILE_NAME}";
        }
        public static string ParsedZoneInputFilePathForRun(string fileStorageLocator)
        {
            return $"{InputFolderPathForRun(fileStorageLocator)}/{PARSED_ZONE_INPUT_FILE_NAME}";
        }
        public static string OutputFolderPathForRun(string fileStorageLocator)
        {
            return $"{fileStorageLocator}/{OUTPUT_FOLDER_NAME}";
        }
        public static string OutputFilePathForRun(string fileStorageLocator, string outputFileName)
        {
            return $"{OutputFolderPathForRun(fileStorageLocator)}/{outputFileName}";
        }
        public static string GenerateInputOutputFilePath(string fileStorageLocator, string outputFileName)
        {
            return $"{GenerateInputOutputFolderPath(fileStorageLocator)}/{outputFileName}";
        }
        public static string GenerateInputOutputFolderPath(string fileStorageLocator)
        {
            return $"{fileStorageLocator}/{GENERATE_INPUT_OUTPUT_FOLDER_NAME}";
        }
        public static string AnalysisOutputFilePath(string fileStorageLocator, string outputFileName)
        {
            return $"{AnalysisOutputFolderPath(fileStorageLocator)}/{outputFileName}";
        }
        public static string AnalysisOutputFolderPath(string fileStorageLocator)
        {
            return $"{fileStorageLocator}/{ANALYSIS_OUTPUT_FOLDER_NAME}";
        }
        public static string ModelOutputFolderPath(string modelName, string fileName)
        {
            return $"{modelName}/{fileName}";
        }
    }
}
