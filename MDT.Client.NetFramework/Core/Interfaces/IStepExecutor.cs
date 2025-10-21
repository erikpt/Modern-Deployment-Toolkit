using MDT.Client.NetFramework.Core.Models;

namespace MDT.Client.NetFramework.Core.Interfaces
{
    /// <summary>
    /// Interface for task sequence step executors
    /// </summary>
    public interface IStepExecutor
    {
        /// <summary>
        /// Gets the step type this executor supports
        /// </summary>
        string SupportedStepType { get; }

        /// <summary>
        /// Determines if this executor can execute the given step
        /// </summary>
        bool CanExecute(TaskSequenceStep step);

        /// <summary>
        /// Executes a task sequence step
        /// </summary>
        StepExecutionResult Execute(TaskSequenceStep step, ExecutionContext context);
    }
}
