using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olsson.GET.Common.Utilities
{
    public class Logging
    {
        static Logging()
        {
            GlobalContext.Properties["assemblyName"] = System.AppDomain.CurrentDomain.FriendlyName;
            XmlConfigurator.Configure();
        }

        public static ILog GetLogger(Type type)
        {
            return LogManager.GetLogger(type);
        }

        public static ILog GetLogger(Type type, string subType)
        {
            return LogManager.GetLogger(type.FullName + "." + subType);
        }
    }
}
