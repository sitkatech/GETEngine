using Olsson.GET.Accessors.Authentication;
using Olsson.GET.Accessors.Notification;
using Olsson.GET.Common.Shared;
using Olsson.GET.Accessors.Customers;
using Olsson.GET.Accessors.Models;
using Olsson.GET.Accessors.Runs;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Accessors.Containers;
using Olsson.GET.Accessors.Queue;
using Olsson.GET.Accessors.APIFunctions;
using Olsson.GET.Accessors.FileResource;
using Olsson.GET.Accessors.GETPage;
using Olsson.GET.Accessors.ReportTemplate;
using Olsson.GET.Accessors.Scenarios;

namespace Olsson.GET.Accessors
{
    public class AccessorFactory : FactoryBase
    {
        public const string LocalFileAccessorKey = "Local";
        public const string RemoteFileAccessorKey = "Local";
        public AccessorFactory()
        {
            AddType<IUserAccessor>(typeof(UserAccessor));
            AddType<IEmailAccessor>(typeof(EmailAccessor));
            AddType<ICustomerAccessor>(typeof(CustomerAccessor));
            AddType<IModelAccessor>(typeof(ModelAccessor));
            AddType<IRunAccessor>(typeof(RunAccessor));
            AddType<IBlobFileAccessor>(typeof(BlobFileAccessor));
            AddType<IContainerAccessor>(typeof(ContainerAccessor));
            AddType<IModelFileAccessorFactory>(typeof(ModelFileAccessorFactory));
            AddType<IQueueAccessor>(typeof(QueueAccessor));
            AddType<IAPIFunctionsAccessor>(typeof(APIFunctionsAccessor));
            AddType<IFileAccessor>(typeof(FileAccessor));
            AddType<IScenarioAccessor>(typeof(ScenarioAccessor));
            AddType<IFileResourceAccessor>(typeof(FileResourceAccessor));
            AddType<IReportTemplateAccessor>(typeof(ReportTemplateAccessor));
            AddType<GETPageAccessor>(typeof(GETPageAccessor));
            AddType<ExternalMapLayerAccessor>(typeof(ExternalMapLayerAccessor));
        }

        public T CreateAccessor<T>() where T : class
        {
            T result = base.GetInstanceForType<T>();

            return result;
        }

    }
}