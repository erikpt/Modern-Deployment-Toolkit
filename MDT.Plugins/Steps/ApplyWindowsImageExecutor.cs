using MDT.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDT.Plugins.Steps;

public class ApplyWindowsImageExecutor : BaseStepExecutor
{
    public ApplyWindowsImageExecutor(ILogger<ApplyWindowsImageExecutor> logger) : base(logger)
    {
    }

    public override StepType SupportedStepType => StepType.ApplyWindowsImage;

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
            Logger.LogInformation("Applying Windows image from WIM file");

            var wimPath = step.Properties.GetValueOrDefault("WimPath", "");
            var imageIndex = step.Properties.GetValueOrDefault("ImageIndex", "1");
            var targetDrive = step.Properties.GetValueOrDefault("TargetDrive", "C:");

            if (string.IsNullOrEmpty(wimPath))
            {
                throw new InvalidOperationException("WimPath property is required");
            }

            Logger.LogInformation(
                "Deploying image {ImageIndex} from {WimPath} to {TargetDrive}",
                imageIndex, wimPath, targetDrive);

            await Task.Delay(100, cancellationToken);

            result.Status = ExecutionStatus.Completed;
            result.OutputVariables["ImageApplied"] = "true";
            result.OutputVariables["TargetDrive"] = targetDrive;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to apply Windows image");
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
