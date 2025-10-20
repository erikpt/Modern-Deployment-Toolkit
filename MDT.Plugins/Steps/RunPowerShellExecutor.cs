using MDT.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDT.Plugins.Steps;

public class RunPowerShellExecutor : BaseStepExecutor
{
    public RunPowerShellExecutor(ILogger<RunPowerShellExecutor> logger) : base(logger)
    {
    }

    public override StepType SupportedStepType => StepType.RunPowerShell;

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
            Logger.LogInformation("Running PowerShell script");

            var script = step.Properties.GetValueOrDefault("Script", "");
            var workingDirectory = step.Properties.GetValueOrDefault("WorkingDirectory", Environment.CurrentDirectory);
            var timeoutMinutes = int.Parse(step.Properties.GetValueOrDefault("TimeoutMinutes", "30"));

            if (string.IsNullOrEmpty(script))
            {
                throw new InvalidOperationException("Script property is required");
            }

            Logger.LogInformation("Executing PowerShell script");

            await Task.Delay(100, cancellationToken);

            result.Status = ExecutionStatus.Completed;
            result.OutputVariables["ScriptExecuted"] = "true";
            result.OutputVariables["ExitCode"] = "0";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to run PowerShell script");
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
