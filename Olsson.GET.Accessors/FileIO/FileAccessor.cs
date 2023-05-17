using Olsson.GET.Common.DataContracts.Files;
using Olsson.GET.Common.Utilities;
using System.Collections.Generic;
using System.IO;

namespace Olsson.GET.Accessors.FileIO
{
    public class FileAccessor : IFileAccessor
    {
        public List<FileModel> GetFilesInModflowDataFolder()
        {
            var files = Directory.GetFiles(ConfigurationHelper.AppSettings.ModflowDataFolder);

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
