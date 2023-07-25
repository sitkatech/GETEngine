using System;
using System.Reflection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;


namespace Olsson.GET.Common.Utilities
{
    public class Logging
    {
        static Logging()
        {
            var assemblyName = AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Name;

            var logPath = ConfigurationHelper.GetEnvironment() == "Production"
                ? "${HOME}\\site\\wwwroot\\App_Data\\Logs\\"
                : "c:\\Logs\\GET\\";

            var outputTemplate =
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{MethodName}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                // add console as logging target
                
                .WriteTo.Console(outputTemplate: outputTemplate)
                // add rolling file logging target
                .WriteTo.File($"{logPath}{assemblyName}.log",
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    rollingInterval: RollingInterval.Day, 
                    fileSizeLimitBytes:10000000,
                    retainedFileCountLimit:10,
                    outputTemplate:outputTemplate)
                // add Azure Table Storage
                .WriteTo.AzureTableStorage(ConfigurationHelper.ConnectionStrings.AzureStorageAccount,
                    storageTableName: "Logs")
                .Enrich.WithCaller()
                // set default minimum level
                .MinimumLevel.Information()
                .CreateLogger();
            
        }

        public static ILogger GetLogger<T>()
        {
            return Log.Logger.ForContext<T>();
        }

        public static ILogger GetLogger()
        {
            return Log.Logger;
        }

    }
}
