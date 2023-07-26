using Microsoft.Extensions.Logging;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Olsson.GET.Engines
{
    public class AnalysisEngine
    {
        private static readonly ILogger Logger = Logging.GetLogger<AnalysisEngine>();
        public AnalysisEngine()
        {
        }

        public AnalysisResult RunAnalysis(ModelExecutable modelExecutable)
        {
            var modelExecutableWorkingDirectory = Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, modelExecutable.WorkingDirectory ?? "");
            var modelExecutableName = Path.GetFullPath(Path.Combine(modelExecutableWorkingDirectory, modelExecutable.ExecutableName));
            var fileName = modelExecutable.WrapWithBatchFile
                ? CreateBatchFile(modelExecutableWorkingDirectory, modelExecutableName)
                : modelExecutableName;
            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                WorkingDirectory = modelExecutableWorkingDirectory,
                UseShellExecute = modelExecutable.UseShellExecute,
                RedirectStandardOutput = modelExecutable.RedirectStandardOutput,
                CreateNoWindow = modelExecutable.CreateNoWindow

            };
            if (!string.IsNullOrWhiteSpace(modelExecutable.Arguments))
            {
                processStartInfo.Arguments = modelExecutable.Arguments;
            }

            var runResult = ProcessStartImpl(processStartInfo, modelExecutableName);
            return runResult;
        }

        private AnalysisResult ProcessStartImpl(ProcessStartInfo processStartInfo, string modelEngineExeName)
        {
            try
            {
                var processStartArgs = new Dictionary<string, string>
                {
                    { "WorkingDirectory", processStartInfo.WorkingDirectory },
                    { "FileName", processStartInfo.FileName },
                    { "Arguments", processStartInfo.Arguments },
                    { "UseShellExecute", processStartInfo.UseShellExecute ? "true" : "false" },
                    { "RedirectStandardOutput", processStartInfo.RedirectStandardOutput ? "true" : "false" },
                    { "CreateNoWindow", processStartInfo.CreateNoWindow ? "true" : "false" }
                };

                var argumentsAsString = string.Join(", ", processStartArgs.Select(x => $"{x.Key}: {x.Value}"));
                Logger.LogInformation($"Starting {modelEngineExeName} with arguments: {argumentsAsString}");
                using (var process = Process.Start(processStartInfo))
                {
                    var consoleOut = new StringBuilder();
                    if (processStartInfo.RedirectStandardOutput)
                    {
                        while (!process.StandardOutput.EndOfStream)
                        {
                            consoleOut.AppendLine(process.StandardOutput.ReadLine());
                        }
                    }

                    process.WaitForExit();

                    Logger.LogInformation($"{modelEngineExeName} exit code: {process.ExitCode}");

                    var analysisResult = new AnalysisResult()
                    {
                        Success = process.ExitCode == 0
                    };
                    if (processStartInfo.RedirectStandardOutput)
                    {
                        analysisResult.ConsoleOutput = consoleOut.ToString();
                    }

                    return analysisResult;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new AnalysisResult() { Success = false };
            }
        }

        /// <summary>
        /// This is very specific to zbud6.exe and zonbud.exe
        /// In the future this should just be pushed to the model container itself
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <param name="executableFileName"></param>
        /// <returns></returns>
        protected string CreateBatchFile(string workingDirectory, string executableFileName)
        {
            // We create batch file because if we directly execute the exe and redirect the standard input, we end up with the UTF-8 BOM on the beginning of our files.
            // This doesn't happen when executing a batch file.
            var batchFileName = Path.Combine(workingDirectory, "zonebudget.generated.bat");
            var inputDataFileName = Path.Combine(workingDirectory, "RunZoneBudget.bat");
            File.WriteAllText(batchFileName, $"{executableFileName} < {inputDataFileName}");
            return batchFileName;
        }
    }
}