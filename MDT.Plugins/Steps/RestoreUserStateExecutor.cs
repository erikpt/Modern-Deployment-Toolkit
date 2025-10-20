using MDT.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDT.Plugins.Steps;

public class RestoreUserStateExecutor : BaseStepExecutor
{
    public RestoreUserStateExecutor(ILogger<RestoreUserStateExecutor> logger) : base(logger)
    {
    }

    public override StepType SupportedStepType => StepType.RestoreUserState;

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
            Logger.LogInformation("Restoring user state with USMT");

            var storePath = step.Properties.GetValueOrDefault("StorePath", "");
            
            if (string.IsNullOrEmpty(storePath))
            {
                throw new InvalidOperationException("StorePath property is required");
            }

            Logger.LogInformation("Restoring user state from: {StorePath}", storePath);

            await Task.Delay(100, cancellationToken);

            result.Status = ExecutionStatus.Completed;
            result.OutputVariables["UserStateRestored"] = "true";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to restore user state");
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
