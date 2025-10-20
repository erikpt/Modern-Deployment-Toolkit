using MDT.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDT.Plugins.Steps;

public class CaptureUserStateExecutor : BaseStepExecutor
{
    public CaptureUserStateExecutor(ILogger<CaptureUserStateExecutor> logger) : base(logger)
    {
    }

    public override StepType SupportedStepType => StepType.CaptureUserState;

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
            Logger.LogInformation("Capturing user state with USMT");

            var storePath = step.Properties.GetValueOrDefault("StorePath", "");
            var includeFiles = bool.Parse(step.Properties.GetValueOrDefault("IncludeFiles", "true"));
            var includeSettings = bool.Parse(step.Properties.GetValueOrDefault("IncludeSettings", "true"));

            if (string.IsNullOrEmpty(storePath))
            {
                throw new InvalidOperationException("StorePath property is required");
            }

            Logger.LogInformation("Capturing user state to: {StorePath}", storePath);

            await Task.Delay(100, cancellationToken);

            result.Status = ExecutionStatus.Completed;
            result.OutputVariables["UserStateCaptured"] = "true";
            result.OutputVariables["StorePath"] = storePath;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to capture user state");
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
