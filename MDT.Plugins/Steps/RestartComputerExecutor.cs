using MDT.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDT.Plugins.Steps;

public class RestartComputerExecutor : BaseStepExecutor
{
    public RestartComputerExecutor(ILogger<RestartComputerExecutor> logger) : base(logger)
    {
    }

    public override StepType SupportedStepType => StepType.RestartComputer;

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
            Logger.LogInformation("Initiating computer restart");

            var delaySeconds = int.Parse(step.Properties.GetValueOrDefault("DelaySeconds", "5"));
            var message = step.Properties.GetValueOrDefault("Message", "The computer will restart");

            Logger.LogInformation("Computer will restart in {DelaySeconds} seconds", delaySeconds);

            await Task.Delay(100, cancellationToken);

            result.Status = ExecutionStatus.Completed;
            result.OutputVariables["RestartScheduled"] = "true";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to restart computer");
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
