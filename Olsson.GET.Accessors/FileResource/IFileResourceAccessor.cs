using Olsson.GET.Common.DataContracts.FileResource;
using System;

namespace Olsson.GET.Accessors.FileResource
{
    public interface IFileResourceAccessor
    {
        FileResourceData FindByGuid(Guid fileResourceGuid);
        FileResourceData FindByID(int fileResourceInfoID);

        FileResourceInfo CreateFileResource(FileResourceCreateDto fileResourceCreateDto);
        FileResourceMimeType FindFileResourceMimeTypeByContentType(string uploadContentType);
    }
}