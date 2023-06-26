using Microsoft.Extensions.Logging;
using Olsson.GET.Common.Shared.Enums;
using Olsson.GET.Common.Utilities;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Runs;
using System;

namespace Olsson.GET.Clients.Agent
{
    class Program
    {
        private static readonly ILogger Logger = Logging.GetLogger<Program>();
        private static ManagerFactory factory = new ManagerFactory();
        private static IRunManager RunManager => factory.CreateManager<IRunManager>();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Logger.LogDebug("Agent started");

            args = new string[2] { "693", "2" };
            Logger.LogInformation($"Loading Native Assemblies from {AppDomain.CurrentDomain.BaseDirectory}");
            // todo: native assemblies
            //SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
            //SqlProviderServices.SqlServerTypesAssemblyName = "Microsoft.SqlServer.Types, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";
            if (args == null || args.Length < 1)
            {
                Logger.LogError("No run id specified");
                Environment.Exit(-1);
            }
            else
            {
                Logger.LogInformation($"Starting run id:{args[0]}");
            }

            var runId = int.Parse(args[0]);
            var processType = (AgentProcessType)int.Parse(args[1]);

            switch (processType)
            {
                case AgentProcessType.Input:
                    Logger.LogInformation($"Generating input for run id:{args[0]}");
                    RunManager.GenerateInputFiles(runId);
                    break;
                case AgentProcessType.Analysis:
                    Logger.LogInformation($"Running analysis for run id:{args[0]}");
                    RunManager.RunAnalysis(runId);

                    Logger.LogInformation($"Generating output for run id:{args[0]}");
                    RunManager.GenerateOutputFiles(runId);
                    break;
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Logger.LogError("Global Error Executing Run", ex);
            }
            else
            {
                Logger.LogError("Global Error Executing Run");
            }
        }
    }
}
