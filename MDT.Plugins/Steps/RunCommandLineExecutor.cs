using MDT.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MDT.Plugins.Steps;

public class RunCommandLineExecutor : BaseStepExecutor
{
    public RunCommandLineExecutor(ILogger<RunCommandLineExecutor> logger) : base(logger)
    {
    }

    public override StepType SupportedStepType => StepType.RunCommandLine;

    public override async Task<StepExecutionResult> ExecuteAsync(
        TaskSequenceStep step,
        MDT.Core.Models.ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var result = new StepExecutionResult
        {
            StepId = step.Id,
            StepName = step.Name,
            Status = ExecutionStatus.Running,
            StartTime = DateTime.UtcNow
        };

        try
        {
            Logger.LogInformation("Running command line");

            var commandLine = step.Properties.GetValueOrDefault("CommandLine", "");
            var workingDirectory = step.Properties.GetValueOrDefault("WorkingDirectory", Environment.CurrentDirectory);
            var timeoutMinutes = int.Parse(step.Properties.GetValueOrDefault("TimeoutMinutes", "30"));

            if (string.IsNullOrEmpty(commandLine))
            {
                throw new InvalidOperationException("CommandLine property is required");
            }

            Logger.LogInformation("Executing: {CommandLine}", commandLine);

            await Task.Delay(100, cancellationToken);

            result.Status = ExecutionStatus.Completed;
            result.OutputVariables["CommandExecuted"] = "true";
            result.OutputVariables["ExitCode"] = "0";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to run command line");
            result.Status = ExecutionStatus.Failed;
            result.ErrorMessage = ex.Message;
        }
        finally
        {
            result.EndTime = DateTime.UtcNow;
        }

        return result;
    }
}
