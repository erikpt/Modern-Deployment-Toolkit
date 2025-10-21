using System;
using System.Collections.Generic;
using System.IO;
using MDT.Client.NetFramework.ApiClient;
using MDT.Client.NetFramework.Core.Interfaces;
using MDT.Client.NetFramework.Core.Models;
using MDT.Client.NetFramework.Core.Services;
using MDT.Client.NetFramework.Engine;
using MDT.Client.NetFramework.Parsers;
using MDT.Client.NetFramework.StepExecutors;

namespace MDT.Client.NetFramework
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("Modern Deployment Toolkit - WinPE Client");
            Console.WriteLine("Version: 1.0.0 (.NET Framework 3.5)");
            Console.WriteLine("========================================");
            Console.WriteLine();

            try
            {
                // Parse command line arguments
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: MDT.Client.exe <task-sequence-file> [server-url]");
                    Console.WriteLine();
                    Console.WriteLine("Arguments:");
                    Console.WriteLine("  task-sequence-file : Path to MDT task sequence XML file");
                    Console.WriteLine("  server-url         : Optional URL of MDT server (e.g., https://server:5001)");
                    Console.WriteLine();
                    Console.WriteLine("Examples:");
                    Console.WriteLine("  MDT.Client.exe C:\\Deploy\\TaskSequence.xml");
                    Console.WriteLine("  MDT.Client.exe C:\\Deploy\\TaskSequence.xml https://mdt.contoso.com");
                    return 1;
                }

                string taskSequenceFile = args[0];
                string serverUrl = args.Length > 1 ? args[1] : null;

                if (!File.Exists(taskSequenceFile))
                {
                    Console.WriteLine("ERROR: Task sequence file not found: {0}", taskSequenceFile);
                    return 1;
                }

                Console.WriteLine("Task Sequence File: {0}", taskSequenceFile);
                if (!string.IsNullOrEmpty(serverUrl))
                {
                    Console.WriteLine("Server URL: {0}", serverUrl);
                }
                Console.WriteLine();

                // Read task sequence file
                string taskSequenceXml = File.ReadAllText(taskSequenceFile);

                // Parse task sequence
                Console.WriteLine("Parsing task sequence...");
                MdtXmlParser parser = new MdtXmlParser();
                
                if (!parser.CanParse(taskSequenceXml))
                {
                    Console.WriteLine("ERROR: Invalid task sequence format");
                    return 1;
                }

                TaskSequence taskSequence = parser.Parse(taskSequenceXml);
                Console.WriteLine("Task Sequence: {0}", taskSequence.Name);
                Console.WriteLine("Version: {0}", taskSequence.Version);
                Console.WriteLine("Steps: {0}", taskSequence.Steps.Count);
                Console.WriteLine();

                // Initialize components
                VariableManager variableManager = new VariableManager();
                ConditionEvaluator conditionEvaluator = new ConditionEvaluator(variableManager);

                // Create server client if URL provided
                IServerClient serverClient = null;
                if (!string.IsNullOrEmpty(serverUrl))
                {
                    serverClient = new MdtServerClient(serverUrl, Guid.NewGuid().ToString());
                }

                // Register step executors
                List<IStepExecutor> executors = new List<IStepExecutor>();
                executors.Add(new SetVariableExecutor(variableManager));
                executors.Add(new RunCommandLineExecutor(variableManager));
                executors.Add(new BddValidateExecutor(variableManager));
                executors.Add(new CheckBiosExecutor(variableManager));
                executors.Add(new PartitionDiskExecutor(variableManager));
                executors.Add(new ConfigureExecutor(variableManager));
                executors.Add(new BddInstallOsExecutor(variableManager));
                executors.Add(new ZtiWinReExecutor(variableManager));
                executors.Add(new RunPowerShellExecutor(variableManager));
                executors.Add(new RebootExecutor(variableManager));

                // Create and execute task sequence engine
                TaskSequenceEngine engine = new TaskSequenceEngine(
                    variableManager,
                    conditionEvaluator,
                    executors,
                    serverClient);

                Console.WriteLine("Starting task sequence execution...");
                Console.WriteLine();

                ExecutionContext result = engine.Execute(taskSequence);

                // Display results
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("Execution Summary");
                Console.WriteLine("========================================");
                Console.WriteLine("Status: {0}", result.Status);
                Console.WriteLine("Started: {0}", result.StartTime);
                Console.WriteLine("Ended: {0}", result.EndTime);
                Console.WriteLine("Duration: {0}", result.EndTime.HasValue ? (result.EndTime.Value - result.StartTime).ToString() : "N/A");
                Console.WriteLine("Total Steps: {0}", result.StepResults.Count);

                int completed = 0;
                int failed = 0;
                foreach (StepExecutionResult stepResult in result.StepResults)
                {
                    if (stepResult.Status == ExecutionStatus.Completed)
                        completed++;
                    else if (stepResult.Status == ExecutionStatus.Failed)
                        failed++;
                }

                Console.WriteLine("Completed: {0}", completed);
                Console.WriteLine("Failed: {0}", failed);
                Console.WriteLine();

                if (failed > 0)
                {
                    Console.WriteLine("Failed Steps:");
                    foreach (StepExecutionResult stepResult in result.StepResults)
                    {
                        if (stepResult.Status == ExecutionStatus.Failed)
                        {
                            Console.WriteLine("  - {0}: {1}", stepResult.StepName, stepResult.ErrorMessage);
                        }
                    }
                    Console.WriteLine();
                }

                return result.Status == ExecutionStatus.Completed ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("FATAL ERROR: {0}", ex.Message);
                Console.WriteLine("Stack Trace: {0}", ex.StackTrace);
                return 1;
            }
        }
    }
}
