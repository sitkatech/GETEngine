using Olsson.GET.Common.DataContracts.FileResource;
using System;

namespace Olsson.GET.Managers.FileResource
{
    public interface IFileResourceManager
    {
        FileResourceData FindByGuid(Guid fileResourceGuid);
        FileResourceData FindByID(int fileResourceInfoID);
        FileResourceMimeType FindFileResourceMimeTypeByContentType(string uploadContentType);
        FileResourceInfo CreateFileResource(FileResourceCreateDto fileResourceCreateDto);
    }
}
