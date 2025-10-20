using MDT.Core.Models;

namespace MDT.Core.Interfaces;

public interface IStepExecutor
{
    StepType SupportedStepType { get; }
    Task<StepExecutionResult> ExecuteAsync(TaskSequenceStep step, Models.ExecutionContext context, CancellationToken cancellationToken = default);
    bool CanExecute(TaskSequenceStep step);
}
