using log4net;
using Olsson.GET.Accessors.FileResource;
using Olsson.GET.Common.DataContracts.FileResource;
using Olsson.GET.Common.Utilities;
using System;
using Microsoft.Extensions.Logging;

namespace Olsson.GET.Managers.FileResource
{
    public class FileResourceManager : BaseManager, IFileResourceManager
    {
        private string AGENT_EXECUTABLE_NAME = $"{ConfigurationHelper.AppSettings.DockerAgentContainerPath}\\Olsson.GET.Clients.Agent.exe";
        private static readonly ILogger Logger = Logging.GetLogger<FileResourceManager>();

        public FileResourceData FindByGuid(Guid fileResourceGuid)
        {
            Logger.LogInformation($"Finding file resource {fileResourceGuid}");
            
            return AccessorFactory.CreateAccessor<IFileResourceAccessor>().FindByGuid(fileResourceGuid);
        }

        public FileResourceData FindByID(int fileResourceInfoID)
        {
            Logger.LogInformation($"Finding file resource {fileResourceInfoID}");

            return AccessorFactory.CreateAccessor<IFileResourceAccessor>().FindByID(fileResourceInfoID);
        }

        public FileResourceMimeType FindFileResourceMimeTypeByContentType(string uploadContentType)
        {
            Logger.LogInformation($"Finding MIME type {uploadContentType}");

            return AccessorFactory.CreateAccessor<IFileResourceAccessor>()
                .FindFileResourceMimeTypeByContentType(uploadContentType);
        }

        public FileResourceInfo CreateFileResource(FileResourceCreateDto fileResourceCreateDto)
        {
            Logger.LogInformation("Creating new File Resource");

            return AccessorFactory.CreateAccessor<IFileResourceAccessor>().CreateFileResource(fileResourceCreateDto);
        }
    }
}