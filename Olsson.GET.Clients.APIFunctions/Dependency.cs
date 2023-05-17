using Autofac;
using Microsoft.Extensions.Logging;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Customers;
using Olsson.GET.Managers.Runs;

namespace Olsson.GET.Clients.APIFunctions
{
    internal class Dependency
    {
        internal static IContainer Container { get; private set; }
        public static void CreateContainer(ILogger logger)
        {
            if (Container == null)
            {
                var builder = new ContainerBuilder();

                builder.RegisterType<ManagerFactory>().As<ManagerFactory>();
                builder.RegisterType<CustomerManager>().As<ICustomerManager>();
                builder.RegisterType<RunManager>().As<IRunManager>();

                builder.RegisterModule(new LoggingModule(logger));

                Container = builder.Build();
            }
        }
    }
}
