using System;
using System.Diagnostics;
using System.Text;
using MDT.Client.NetFramework.Core.Models;
using MDT.Client.NetFramework.Core.Services;

namespace MDT.Client.NetFramework.StepExecutors
{
    /// <summary>
    /// Executes SMS_TaskSequence_RunCommandLineAction step
    /// Runs a command line program
    /// </summary>
    public class RunCommandLineExecutor : BaseStepExecutor
    {
        public RunCommandLineExecutor(VariableManager variableManager)
            : base(variableManager)
        {
        }

        public override string SupportedStepType
        {
            get { return "SMS_TaskSequence_RunCommandLineAction"; }
        }

        public override StepExecutionResult Execute(TaskSequenceStep step, ExecutionContext context)
        {
            StepExecutionResult result = new StepExecutionResult
            {
                StepId = step.Id,
                StepName = step.Name,
                StartTime = DateTime.UtcNow,
                Status = ExecutionStatus.Running
            };

            try
            {
                string commandLine = GetProperty(step, "CommandLine");
                string workingDirectory = GetProperty(step, "WorkingDirectory", Environment.CurrentDirectory);
                string timeoutStr = GetProperty(step, "Timeout", "3600"); // Default 1 hour

                if (string.IsNullOrEmpty(commandLine))
                {
                    result.Status = ExecutionStatus.Failed;
                    result.ErrorMessage = "CommandLine property is required";
                    result.ExitCode = 1;
                    return result;
                }

                int timeout;
                if (!int.TryParse(timeoutStr, out timeout))
                {
                    timeout = 3600; // Default to 1 hour
                }

                Log(string.Format("Executing command: {0}", commandLine));
                Log(string.Format("Working directory: {0}", workingDirectory));

                // Parse command line into executable and arguments
                string executable;
                string arguments;
                ParseCommandLine(commandLine, out executable, out arguments);

                // Create process
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                using (Process process = new Process())
                {
                    process.StartInfo = psi;
                    
                    process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            output.AppendLine(e.Data);
                            Log("OUT: " + e.Data);
                        }
                    };

                    process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            error.AppendLine(e.Data);
                            Log("ERR: " + e.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    bool exited = process.WaitForExit(timeout * 1000); // Convert to milliseconds

                    if (!exited)
                    {
                        Log("Process timeout - killing process");
                        try
                        {
                            process.Kill();
                        }
                        catch { }
                        
                        result.Status = ExecutionStatus.Failed;
                        result.ErrorMessage = "Command execution timed out";
                        result.ExitCode = -1;
                        return result;
                    }

                    result.ExitCode = process.ExitCode;
                    
                    Log(string.Format("Command completed with exit code: {0}", result.ExitCode));

                    // Store output in context variables
                    if (output.Length > 0)
                    {
                        result.OutputVariables["Output"] = output.ToString();
                    }

                    // Determine success/failure based on exit code
                    string successCodes = GetProperty(step, "SuccessCodes", "0");
                    if (IsSuccessCode(result.ExitCode, successCodes))
                    {
                        result.Status = ExecutionStatus.Completed;
                    }
                    else
                    {
                        result.Status = ExecutionStatus.Failed;
                        result.ErrorMessage = string.Format("Command exited with code {0}. Error: {1}", 
                            result.ExitCode, error.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Error executing command: " + ex.Message);
                result.Status = ExecutionStatus.Failed;
                result.ErrorMessage = ex.Message;
                result.ExitCode = 1;
            }
            finally
            {
                result.EndTime = DateTime.UtcNow;
            }

            return result;
        }

        private void ParseCommandLine(string commandLine, out string executable, out string arguments)
        {
            commandLine = commandLine.Trim();

            // Handle quoted executable
            if (commandLine.StartsWith("\""))
            {
                int endQuote = commandLine.IndexOf('"', 1);
                if (endQuote > 0)
                {
                    executable = commandLine.Substring(1, endQuote - 1);
                    arguments = commandLine.Substring(endQuote + 1).Trim();
                    return;
                }
            }

            // Handle unquoted executable
            int firstSpace = commandLine.IndexOf(' ');
            if (firstSpace > 0)
            {
                executable = commandLine.Substring(0, firstSpace);
                arguments = commandLine.Substring(firstSpace + 1).Trim();
            }
            else
            {
                executable = commandLine;
                arguments = string.Empty;
            }
        }

        private bool IsSuccessCode(int exitCode, string successCodes)
        {
            if (string.IsNullOrEmpty(successCodes))
                return exitCode == 0;

            string[] codes = successCodes.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string code in codes)
            {
                int successCode;
                if (int.TryParse(code.Trim(), out successCode) && successCode == exitCode)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
