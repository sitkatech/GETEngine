using log4net;
using Olsson.GET.Accessors.FileResource;
using Olsson.GET.Common.DataContracts.FileResource;
using Olsson.GET.Common.Utilities;
using System;

namespace Olsson.GET.Managers.FileResource
{
    public class FileResourceManager : BaseManager, IFileResourceManager
    {
        private string AGENT_EXECUTABLE_NAME = $"{ConfigurationHelper.AppSettings.DockerAgentContainerPath}\\Olsson.GET.Clients.Agent.exe";
        private static readonly ILog Logger = Logging.GetLogger(typeof(FileResourceManager));

        public FileResourceData FindByGuid(Guid fileResourceGuid)
        {
            Logger.Info($"Finding file resource {fileResourceGuid}");
            
            return AccessorFactory.CreateAccessor<IFileResourceAccessor>().FindByGuid(fileResourceGuid);
        }

        public FileResourceData FindByID(int fileResourceInfoID)
        {
            Logger.Info($"Finding file resource {fileResourceInfoID}");

            return AccessorFactory.CreateAccessor<IFileResourceAccessor>().FindByID(fileResourceInfoID);
        }

        public FileResourceMimeType FindFileResourceMimeTypeByContentType(string uploadContentType)
        {
            Logger.Info($"Finding MIME type {uploadContentType}");

            return AccessorFactory.CreateAccessor<IFileResourceAccessor>()
                .FindFileResourceMimeTypeByContentType(uploadContentType);
        }

        public FileResourceInfo CreateFileResource(FileResourceCreateDto fileResourceCreateDto)
        {
            Logger.Info("Creating new File Resource");

            return AccessorFactory.CreateAccessor<IFileResourceAccessor>().CreateFileResource(fileResourceCreateDto);
        }
    }
}