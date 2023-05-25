using Olsson.GET.Common.DataContracts.Files;
using System.Collections.Generic;

namespace Olsson.GET.Accessors.FileIO
{
    public interface IFileAccessor
    {
        List<FileModel> GetFilesInModflowDataFolder();

        void DeleteFile(string path);
    }
}
