using System;
using MDT.Client.NetFramework.Core.Models;
using MDT.Client.NetFramework.Core.Services;

namespace MDT.Client.NetFramework.StepExecutors
{
    /// <summary>
    /// Executes SMS_TaskSequence_SetVariableAction step
    /// Sets a task sequence variable
    /// </summary>
    public class SetVariableExecutor : BaseStepExecutor
    {
        public SetVariableExecutor(VariableManager variableManager)
            : base(variableManager)
        {
        }

        public override string SupportedStepType
        {
            get { return "SMS_TaskSequence_SetVariableAction"; }
        }

        public override StepExecutionResult Execute(TaskSequenceStep step, ExecutionContext context)
        {
            StepExecutionResult result = new StepExecutionResult
            {
                StepId = step.Id,
                StepName = step.Name,
                StartTime = DateTime.UtcNow,
                Status = ExecutionStatus.Running
            };

            try
            {
                string variableName = GetProperty(step, "VariableName");
                string variableValue = GetProperty(step, "VariableValue");

                if (string.IsNullOrEmpty(variableName))
                {
                    result.Status = ExecutionStatus.Failed;
                    result.ErrorMessage = "VariableName property is required";
                    result.ExitCode = 1;
                    return result;
                }

                Log(string.Format("Setting variable: {0} = {1}", variableName, variableValue));

                VariableManager.SetVariable(variableName, variableValue);
                context.Variables[variableName] = variableValue;

                result.Status = ExecutionStatus.Completed;
                result.ExitCode = 0;
                result.OutputVariables[variableName] = variableValue;
            }
            catch (Exception ex)
            {
                Log("Error setting variable: " + ex.Message);
                result.Status = ExecutionStatus.Failed;
                result.ErrorMessage = ex.Message;
                result.ExitCode = 1;
            }
            finally
            {
                result.EndTime = DateTime.UtcNow;
            }

            return result;
        }
    }
}
