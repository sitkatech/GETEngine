using Olsson.GET.Accessors;
using Olsson.GET.Common.Shared;
using Olsson.GET.Engines;
using Olsson.GET.Managers.Authentication;
using Olsson.GET.Managers.Customers;
using Olsson.GET.Managers.FileResource;
using Olsson.GET.Managers.Notification;
using Olsson.GET.Managers.Models;
using Olsson.GET.Managers.ReportTemplate;
using Olsson.GET.Managers.Runs;
using Olsson.GET.Managers.Scenarios;

namespace Olsson.GET.Managers
{
    public class ManagerFactory : FactoryBase
    {
        private AccessorFactory _accessorFactory;
        private EngineFactory _engineFactory;

        public ManagerFactory() : this(null, null)
        {
        }

        public ManagerFactory(AccessorFactory accessorFactory, EngineFactory engineFactory)
        {
            _accessorFactory = accessorFactory ?? new AccessorFactory();
            _engineFactory = engineFactory ?? new EngineFactory(_accessorFactory);

            AddType<IAuthenticationManager>(typeof(AuthenticationManager));
            AddType<INotificationManager>(typeof(NotificationManager));
            AddType<ICustomerManager>(typeof(CustomerManager));
            AddType<IModelManager>(typeof(ModelManager));
            AddType<IRunManager>(typeof(RunManager));
            AddType<IScenarioManager>(typeof(ScenarioManager));
            AddType<IFileResourceManager>(typeof(FileResourceManager));
            AddType<IReportTemplateManager>(typeof(ReportTemplateManager));
        }

        public T CreateManager<T>() where T : class
        {
            return CreateManager<T>(null, null);
        }

        public T CreateManager<T>(AccessorFactory accessorFactory, EngineFactory engineFactory) where T : class
        {
            _accessorFactory = accessorFactory ?? _accessorFactory;
            _engineFactory = _engineFactory ?? new EngineFactory(_accessorFactory);

            T result = GetInstanceForType<T>();

            // configure the context and the accessor factory if the result is not a mock
            if (result is BaseManager)
            {
                (result as BaseManager).AccessorFactory = _accessorFactory;
                (result as BaseManager).EngineFactory = _engineFactory;
                (result as BaseManager).ManagerFactory = this;
            }

            return result;
        }
    }
}
