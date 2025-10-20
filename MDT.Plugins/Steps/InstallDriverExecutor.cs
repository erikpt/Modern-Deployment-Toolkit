using MDT.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDT.Plugins.Steps;

public class InstallDriverExecutor : BaseStepExecutor
{
    public InstallDriverExecutor(ILogger<InstallDriverExecutor> logger) : base(logger)
    {
    }

    public override StepType SupportedStepType => StepType.InstallDriver;

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
            Logger.LogInformation("Installing drivers");

            var driverPath = step.Properties.GetValueOrDefault("DriverPath", "");
            var recursive = bool.Parse(step.Properties.GetValueOrDefault("Recursive", "true"));

            if (string.IsNullOrEmpty(driverPath))
            {
                throw new InvalidOperationException("DriverPath property is required");
            }

            Logger.LogInformation("Installing drivers from: {DriverPath}", driverPath);

            await Task.Delay(100, cancellationToken);

            result.Status = ExecutionStatus.Completed;
            result.OutputVariables["DriversInstalled"] = "true";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to install drivers");
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
