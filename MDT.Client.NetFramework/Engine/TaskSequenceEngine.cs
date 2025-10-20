using System;
using System.Collections.Generic;
using MDT.Client.NetFramework.Core.Interfaces;
using MDT.Client.NetFramework.Core.Models;
using MDT.Client.NetFramework.Core.Services;

namespace MDT.Client.NetFramework.Engine
{
    /// <summary>
    /// Executes task sequences in WinPE
    /// </summary>
    public class TaskSequenceEngine
    {
        private readonly VariableManager _variableManager;
        private readonly ConditionEvaluator _conditionEvaluator;
        private readonly List<IStepExecutor> _stepExecutors;
        private readonly IServerClient _serverClient;

        public TaskSequenceEngine(
            VariableManager variableManager,
            ConditionEvaluator conditionEvaluator,
            List<IStepExecutor> stepExecutors,
            IServerClient serverClient)
        {
            _variableManager = variableManager ?? throw new ArgumentNullException("variableManager");
            _conditionEvaluator = conditionEvaluator ?? throw new ArgumentNullException("conditionEvaluator");
            _stepExecutors = stepExecutors ?? throw new ArgumentNullException("stepExecutors");
            _serverClient = serverClient;
        }

        /// <summary>
        /// Executes a task sequence
        /// </summary>
        public ExecutionContext Execute(TaskSequence taskSequence)
        {
            if (taskSequence == null)
                throw new ArgumentNullException("taskSequence");

            ExecutionContext context = new ExecutionContext
            {
                TaskSequenceId = taskSequence.Id,
                Status = ExecutionStatus.Running,
                StartTime = DateTime.UtcNow
            };

            try
            {
                Log("Starting task sequence: " + taskSequence.Name);

                // Initialize variables
                InitializeVariables(taskSequence, context);

                // Execute steps
                foreach (TaskSequenceStep step in taskSequence.Steps)
                {
                    StepExecutionResult result = ExecuteStep(step, context);
                    context.StepResults.Add(result);

                    // Report progress to server
                    if (_serverClient != null)
                    {
                        _serverClient.ReportStepComplete(result);
                        _serverClient.ReportProgress(context);
                    }

                    // Check if we should stop execution
                    if (result.Status == ExecutionStatus.Failed && !step.ContinueOnError)
                    {
                        context.Status = ExecutionStatus.Failed;
                        Log("Task sequence failed at step: " + step.Name);
                        break;
                    }
                }

                // Mark as completed if still running
                if (context.Status == ExecutionStatus.Running)
                {
                    context.Status = ExecutionStatus.Completed;
                    Log("Task sequence completed successfully");
                }
            }
            catch (Exception ex)
            {
                Log("Task sequence execution error: " + ex.Message);
                context.Status = ExecutionStatus.Failed;
            }
            finally
            {
                context.EndTime = DateTime.UtcNow;
                
                // Final progress report
                if (_serverClient != null)
                {
                    _serverClient.ReportProgress(context);
                }
            }

            return context;
        }

        private void InitializeVariables(TaskSequence taskSequence, ExecutionContext context)
        {
            // Set task sequence variables
            if (taskSequence.Variables != null)
            {
                foreach (TaskSequenceVariable variable in taskSequence.Variables)
                {
                    _variableManager.SetVariable(variable.Name, variable.Value);
                    context.Variables[variable.Name] = variable.Value;
                }
            }

            // Set built-in variables
            _variableManager.SetReadOnlyVariable("TaskSequenceID", taskSequence.Id);
            _variableManager.SetReadOnlyVariable("TaskSequenceName", taskSequence.Name);
            _variableManager.SetReadOnlyVariable("TaskSequenceVersion", taskSequence.Version);
            _variableManager.SetReadOnlyVariable("_SMSTSMachineName", Environment.MachineName);
        }

        private StepExecutionResult ExecuteStep(TaskSequenceStep step, ExecutionContext context)
        {
            context.CurrentStepId = step.Id;

            StepExecutionResult result = new StepExecutionResult
            {
                StepId = step.Id,
                StepName = step.Name,
                StartTime = DateTime.UtcNow,
                Status = ExecutionStatus.Running
            };

            try
            {
                // Check if step is enabled
                if (!step.Enabled)
                {
                    Log("Step disabled: " + step.Name);
                    result.Status = ExecutionStatus.Completed;
                    return result;
                }

                // Evaluate conditions
                if (!_conditionEvaluator.EvaluateConditions(step.Conditions))
                {
                    Log("Step conditions not met: " + step.Name);
                    result.Status = ExecutionStatus.Completed;
                    return result;
                }

                Log("Executing step: " + step.Name + " (Type: " + step.Type + ")");

                // Handle group steps
                if (string.Equals(step.Type, "group", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(step.Type, "SMS_TaskSequence_Group", StringComparison.OrdinalIgnoreCase))
                {
                    result = ExecuteGroupStep(step, context);
                }
                else
                {
                    // Find and execute step executor
                    IStepExecutor executor = FindExecutor(step);
                    if (executor != null)
                    {
                        result = executor.Execute(step, context);
                    }
                    else
                    {
                        Log("No executor found for step type: " + step.Type);
                        result.Status = ExecutionStatus.Failed;
                        result.ErrorMessage = "No executor available for step type: " + step.Type;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Step execution error: " + ex.Message);
                result.Status = ExecutionStatus.Failed;
                result.ErrorMessage = ex.Message;
            }
            finally
            {
                result.EndTime = DateTime.UtcNow;
            }

            return result;
        }

        private StepExecutionResult ExecuteGroupStep(TaskSequenceStep groupStep, ExecutionContext context)
        {
            StepExecutionResult groupResult = new StepExecutionResult
            {
                StepId = groupStep.Id,
                StepName = groupStep.Name,
                StartTime = DateTime.UtcNow,
                Status = ExecutionStatus.Running
            };

            try
            {
                foreach (TaskSequenceStep childStep in groupStep.ChildSteps)
                {
                    StepExecutionResult childResult = ExecuteStep(childStep, context);
                    context.StepResults.Add(childResult);

                    if (childResult.Status == ExecutionStatus.Failed && !childStep.ContinueOnError)
                    {
                        groupResult.Status = ExecutionStatus.Failed;
                        groupResult.ErrorMessage = "Child step failed: " + childStep.Name;
                        break;
                    }
                }

                if (groupResult.Status == ExecutionStatus.Running)
                {
                    groupResult.Status = ExecutionStatus.Completed;
                }
            }
            catch (Exception ex)
            {
                groupResult.Status = ExecutionStatus.Failed;
                groupResult.ErrorMessage = ex.Message;
            }
            finally
            {
                groupResult.EndTime = DateTime.UtcNow;
            }

            return groupResult;
        }

        private IStepExecutor FindExecutor(TaskSequenceStep step)
        {
            if (_stepExecutors == null)
                return null;

            foreach (IStepExecutor executor in _stepExecutors)
            {
                if (executor.CanExecute(step))
                    return executor;
            }

            return null;
        }

        private void Log(string message)
        {
            string logMessage = string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message);
            Console.WriteLine(logMessage);

            if (_serverClient != null)
            {
                _serverClient.SendLog("Info", message);
            }
        }
    }
}
