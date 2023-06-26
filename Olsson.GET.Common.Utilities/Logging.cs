using Microsoft.Extensions.Logging;


namespace Olsson.GET.Common.Utilities
{
    public class Logging
    {
        private static readonly ILoggerFactory _loggerFactory;
        static Logging()
        {
           
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Default", LogLevel.Information)
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddConsole();
            });
        }

        

        public static ILogger GetLogger<T>()
        {
            return _loggerFactory.CreateLogger<T>();
        }

        //public static ILog GetLogger(Type type, string subType)
        //{
        //    return LogManager.GetLogger(Assembly.GetEntryAssembly(),type.FullName + "." + subType);
        //}
    }
}
