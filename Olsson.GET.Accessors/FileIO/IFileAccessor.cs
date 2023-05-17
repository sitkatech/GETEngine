using Olsson.GET.Common.DataContracts.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olsson.GET.Accessors.FileIO
{
    public interface IFileAccessor
    {
        List<FileModel> GetFilesInModflowDataFolder();

        void DeleteFile(string path);
    }
}
