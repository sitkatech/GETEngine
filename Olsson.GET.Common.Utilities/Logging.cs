using log4net;
using log4net.Config;
using System;
using System.Reflection;

namespace Olsson.GET.Common.Utilities
{
    public class Logging
    {
        static Logging()
        {
            GlobalContext.Properties["assemblyName"] = System.AppDomain.CurrentDomain.FriendlyName;
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()));
        }

        public static ILog GetLogger(Type type)
        {
            return LogManager.GetLogger(type);
        }

        public static ILog GetLogger(Type type, string subType)
        {
            return LogManager.GetLogger(Assembly.GetEntryAssembly(),type.FullName + "." + subType);
        }
    }
}
