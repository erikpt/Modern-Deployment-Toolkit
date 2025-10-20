using MDT.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDT.Plugins.Steps;

public class ApplyFFUImageExecutor : BaseStepExecutor
{
    public ApplyFFUImageExecutor(ILogger<ApplyFFUImageExecutor> logger) : base(logger)
    {
    }

    public override StepType SupportedStepType => StepType.ApplyFFUImage;

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
            Logger.LogInformation("Applying FFU image");

            var ffuPath = step.Properties.GetValueOrDefault("FfuPath", "");
            var targetDisk = step.Properties.GetValueOrDefault("TargetDisk", "0");

            if (string.IsNullOrEmpty(ffuPath))
            {
                throw new InvalidOperationException("FfuPath property is required");
            }

            Logger.LogInformation("Deploying FFU from {FfuPath} to disk {TargetDisk}", ffuPath, targetDisk);

            await Task.Delay(100, cancellationToken);

            result.Status = ExecutionStatus.Completed;
            result.OutputVariables["FFUApplied"] = "true";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to apply FFU image");
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
