using System;
using MDT.Client.NetFramework.Core.Interfaces;
using MDT.Client.NetFramework.Core.Models;
using MDT.Client.NetFramework.Core.Services;

namespace MDT.Client.NetFramework.StepExecutors
{
    /// <summary>
    /// Base class for step executors
    /// </summary>
    public abstract class BaseStepExecutor : IStepExecutor
    {
        protected VariableManager VariableManager { get; private set; }

        protected BaseStepExecutor(VariableManager variableManager)
        {
            VariableManager = variableManager ?? throw new ArgumentNullException("variableManager");
        }

        public abstract string SupportedStepType { get; }

        public virtual bool CanExecute(TaskSequenceStep step)
        {
            if (step == null)
                return false;

            return string.Equals(step.Type, SupportedStepType, StringComparison.OrdinalIgnoreCase);
        }

        public abstract StepExecutionResult Execute(TaskSequenceStep step, ExecutionContext context);

        protected StepExecutionResult CreateSuccessResult(TaskSequenceStep step)
        {
            return new StepExecutionResult
            {
                StepId = step.Id,
                StepName = step.Name,
                Status = ExecutionStatus.Completed,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                ExitCode = 0
            };
        }

        protected StepExecutionResult CreateFailureResult(TaskSequenceStep step, string errorMessage, int exitCode = 1)
        {
            return new StepExecutionResult
            {
                StepId = step.Id,
                StepName = step.Name,
                Status = ExecutionStatus.Failed,
                ErrorMessage = errorMessage,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                ExitCode = exitCode
            };
        }

        protected string GetProperty(TaskSequenceStep step, string propertyName)
        {
            return GetProperty(step, propertyName, string.Empty);
        }

        protected string GetProperty(TaskSequenceStep step, string propertyName, string defaultValue)
        {
            if (step == null || step.Properties == null)
                return defaultValue;

            string value;
            if (step.Properties.TryGetValue(propertyName, out value))
            {
                // Expand variables
                return VariableManager.ExpandVariables(value);
            }

            return defaultValue;
        }

        protected void Log(string message)
        {
            Console.WriteLine("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message);
        }
    }
}
