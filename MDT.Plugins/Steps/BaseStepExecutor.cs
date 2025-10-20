using MDT.Core.Interfaces;
using MDT.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDT.Plugins.Steps;

public abstract class BaseStepExecutor : IStepExecutor
{
    protected readonly ILogger Logger;

    protected BaseStepExecutor(ILogger logger)
    {
        Logger = logger;
    }

    public abstract StepType SupportedStepType { get; }

    public virtual bool CanExecute(TaskSequenceStep step)
    {
        return step.Type == SupportedStepType;
    }

    public abstract Task<StepExecutionResult> ExecuteAsync(
        TaskSequenceStep step,
        MDT.Core.Models.ExecutionContext context,
        CancellationToken cancellationToken = default);

    protected StepExecutionResult CreateSuccessResult(TaskSequenceStep step)
    {
        return new StepExecutionResult
        {
            StepId = step.Id,
            StepName = step.Name,
            Status = ExecutionStatus.Completed,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow
        };
    }

    protected StepExecutionResult CreateFailureResult(TaskSequenceStep step, string errorMessage)
    {
        return new StepExecutionResult
        {
            StepId = step.Id,
            StepName = step.Name,
            Status = ExecutionStatus.Failed,
            ErrorMessage = errorMessage,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow
        };
    }
}
