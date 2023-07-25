using Olsson.GET.Common.DataContracts.Files;
using Olsson.GET.Common.Utilities;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using Serilog;

namespace Olsson.GET.Accessors.FileIO
{
    public class FileAccessor : IFileAccessor
    {
        private static readonly ILogger _logger = Logging.GetLogger<FileAccessor>();
        public List<FileModel> GetFilesInModflowDataFolder()
        {
            var files = Directory.GetFiles(ConfigurationHelper.AppSettings.ModflowDataFolder);

            _logger.Information($"Found {files.Length} files in {ConfigurationHelper.AppSettings.ModflowDataFolder}. {string.Join(", ", files)} ");

            var models = new List<FileModel>();

            foreach (var file in files)
            {
                models.Add(new FileModel
                {
                    Name = Path.GetFileName(file),
                    Path = file,
                    ModDate = File.GetLastWriteTimeUtc(file)
                });
            }

            return models;
        }

        public void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
