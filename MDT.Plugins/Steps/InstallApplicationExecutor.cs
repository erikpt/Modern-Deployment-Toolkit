using MDT.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDT.Plugins.Steps;

public class InstallApplicationExecutor : BaseStepExecutor
{
    public InstallApplicationExecutor(ILogger<InstallApplicationExecutor> logger) : base(logger)
    {
    }

    public override StepType SupportedStepType => StepType.InstallApplication;

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
            Logger.LogInformation("Installing application");

            var appName = step.Properties.GetValueOrDefault("ApplicationName", "");
            var commandLine = step.Properties.GetValueOrDefault("CommandLine", "");
            var workingDirectory = step.Properties.GetValueOrDefault("WorkingDirectory", "");

            if (string.IsNullOrEmpty(appName))
            {
                throw new InvalidOperationException("ApplicationName property is required");
            }

            Logger.LogInformation("Installing application: {ApplicationName}", appName);

            await Task.Delay(100, cancellationToken);

            result.Status = ExecutionStatus.Completed;
            result.OutputVariables["ApplicationInstalled"] = appName;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to install application");
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
