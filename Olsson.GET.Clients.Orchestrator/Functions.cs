using System;
using Microsoft.Azure.WebJobs;
using Olsson.GET.Common.Utilities;
using log4net;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Runs;
using Olsson.GET.Common.Shared.Extensions;
using Olsson.GET.Common.Shared.Enums;
using System.Threading.Tasks;

namespace Olsson.GET.Clients.Orchestrator
{
    public class Functions
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(Functions));
        private static readonly ManagerFactory factory = new ManagerFactory();
        private static IRunManager RunManager => factory.CreateManager<IRunManager>();
#if DEBUG
        private const string CleanExitedContainersCronSchedule = "0 0/15 * * * *"; //every 15 minutes in debug
        private const string FailLongProcessingRunsCronSchedule = "0 0/15 * * * *"; //every 15 minutes in debug
#else
        private const string CleanExitedContainersCronSchedule = "0 0 * * * *"; // hourly
        private const string FailLongProcessingRunsCronSchedule = "0 0 * * * *"; // hourly
#endif

        public static async Task GenerateInputs([QueueTrigger("generateinputsqueue")] string runId)
        {
            try
            {
                Logger.Info($"GenerateInputs Started [{runId}]");

                await RunManager.StartContainer(int.Parse(runId), AgentProcessType.Input);

                Logger.Info($"GenerateInputs Completed [{runId}]");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Console.Write(ex.AllExceptionMessages());
            }
        }

        public static async Task RunAnalysis([QueueTrigger("runanalysisqueue")] string runId)
        {
            try
            {
                Logger.Info("Test to see if changes are being propagated");
                Logger.Info($"RunAnalysis Started [{runId}]");

                await RunManager.StartContainer(int.Parse(runId), AgentProcessType.Analysis);

                Logger.Info($"RunAnalysis Completed [{runId}]");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Console.Write(ex.AllExceptionMessages());
            }
        }

        public static async Task GenerateOutputs([QueueTrigger("generateoutputsqueue")] string runId)
        {
            try
            {
                Logger.Info($"GenerateOutputs Started [{runId}]");

                await RunManager.StartContainer(int.Parse(runId), AgentProcessType.Output);

                Logger.Info($"GenerateOutputs Completed [{runId}]");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Console.Write(ex.AllExceptionMessages());
            }
        }

        public static async Task CleanExitedContainers([TimerTrigger(CleanExitedContainersCronSchedule)] TimerInfo timer)
        {
            try
            {
                Logger.Info("CleanExitedContainers Started");

                await RunManager.CleanCompletedRuns();

                Logger.Info("CleanExitedContainers Completed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Console.Write(ex.AllExceptionMessages());
            }

        }

        public static async Task FailLongProcessingRuns([TimerTrigger(FailLongProcessingRunsCronSchedule)] TimerInfo timer)
        {
            try
            {
                Logger.Info("FailLongProcessingRuns Started");

                await RunManager.FailLongProcessingRuns();

                Logger.Info("FailLongProcessingRuns Completed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Console.Write(ex.AllExceptionMessages());
            }

        }
    }
}
