using Olsson.GET.Common.DataContracts.FileResource;
using System;
using System.Linq;
using Olsson.GET.Accessors.EntityFramework;
using FileResourceData = Olsson.GET.Common.DataContracts.FileResource.FileResourceData;
using FileResourceInfo = Olsson.GET.Common.DataContracts.FileResource.FileResourceInfo;
using FileResourceMimeType = Olsson.GET.Common.DataContracts.FileResource.FileResourceMimeType;

namespace Olsson.GET.Accessors.FileResource
{
    internal class FileResourceAccessor: BaseTableAccessor, IFileResourceAccessor
    {
        public FileResourceData FindByGuid(Guid fileResourceGuid)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var fileResourceData = db.FileResourceDatas.Include("FileResourceInfo")
                    .SingleOrDefault(x => x.FileResourceInfo.FileResourceGUID == fileResourceGuid);

                return DTOMapper.Mapper.Map<FileResourceData>(fileResourceData);
            }
        }

        public FileResourceData FindByID(int fileResourceInfoID)
        {

            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var fileResourceData = db.FileResourceDatas.Include("FileResourceInfo")
                    .SingleOrDefault(x => x.FileResourceInfo.FileResourceInfoID == fileResourceInfoID);

                return DTOMapper.Mapper.Map<FileResourceData>(fileResourceData);
            }
        }

        public FileResourceInfo CreateFileResource(FileResourceCreateDto fileResourceCreateDto)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                  EntityFramework.FileResourceInfo fileResourceInfo = new EntityFramework.FileResourceInfo()
                {
                    CreateDate = DateTime.Now,
                    FileResourceGUID = Guid.NewGuid(),
                    FileResourceMimeTypeID = fileResourceCreateDto.FileResourceMimeType.FileResourceMimeTypeID,
                    OriginalBaseFilename = fileResourceCreateDto.OriginalFilename,
                    OriginalFileExtension = fileResourceCreateDto.OriginalFileExtension,
                    UserID = fileResourceCreateDto.UserID
                };

                db.FileResourceInfos.Add(fileResourceInfo);
                db.SaveChanges();

                var fileResourceData = new EntityFramework.FileResourceData()
                {
                    Data = fileResourceCreateDto.Data,
                    FileResourceInfoID = fileResourceInfo.FileResourceInfoID

                };

                db.FileResourceDatas.Add(fileResourceData);
                db.SaveChanges();

                return DTOMapper.Mapper.Map<FileResourceInfo>(fileResourceInfo);
            }
        }

        public FileResourceMimeType FindFileResourceMimeTypeByContentType(string uploadContentType)
        {
            var fileResourceMimeType = EntityFramework.FileResourceMimeType.All.SingleOrDefault(x =>
                x.FileResourceMimeTypeContentTypeName == uploadContentType);

            return DTOMapper.Mapper.Map<FileResourceMimeType>(fileResourceMimeType);
        }
    }
}
