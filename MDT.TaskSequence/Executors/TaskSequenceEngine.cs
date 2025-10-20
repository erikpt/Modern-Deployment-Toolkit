using MDT.Core.Interfaces;
using MDT.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDT.TaskSequence.Executors;

public class TaskSequenceEngine
{
    private readonly IEnumerable<IStepExecutor> _stepExecutors;
    private readonly IVariableManager _variableManager;
    private readonly IConditionEvaluator _conditionEvaluator;
    private readonly ILogger<TaskSequenceEngine> _logger;

    public TaskSequenceEngine(
        IEnumerable<IStepExecutor> stepExecutors,
        IVariableManager variableManager,
        IConditionEvaluator conditionEvaluator,
        ILogger<TaskSequenceEngine> logger)
    {
        _stepExecutors = stepExecutors;
        _variableManager = variableManager;
        _conditionEvaluator = conditionEvaluator;
        _logger = logger;
    }

    public async Task<MDT.Core.Models.ExecutionContext> ExecuteAsync(
        Core.Models.TaskSequence taskSequence,
        CancellationToken cancellationToken = default)
    {
        var context = new MDT.Core.Models.ExecutionContext
        {
            TaskSequenceId = taskSequence.Id,
            Status = ExecutionStatus.Running,
            StartTime = DateTime.UtcNow
        };

        _logger.LogInformation("Starting task sequence execution: {TaskSequenceName}", taskSequence.Name);

        try
        {
            foreach (var variable in taskSequence.Variables)
            {
                _variableManager.SetVariable(variable.Name, variable.Value);
                context.Variables[variable.Name] = variable.Value;
            }

            foreach (var step in taskSequence.Steps)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    context.Status = ExecutionStatus.Cancelled;
                    break;
                }

                var result = await ExecuteStepAsync(step, context, cancellationToken);
                context.StepResults.Add(result);

                if (result.Status == ExecutionStatus.Failed && !step.ContinueOnError)
                {
                    context.Status = ExecutionStatus.Failed;
                    break;
                }
            }

            if (context.Status == ExecutionStatus.Running)
            {
                context.Status = ExecutionStatus.Completed;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Task sequence execution failed");
            context.Status = ExecutionStatus.Failed;
        }
        finally
        {
            context.EndTime = DateTime.UtcNow;
            _logger.LogInformation(
                "Task sequence execution completed with status: {Status}",
                context.Status);
        }

        return context;
    }

    private async Task<StepExecutionResult> ExecuteStepAsync(
        TaskSequenceStep step,
        MDT.Core.Models.ExecutionContext context,
        CancellationToken cancellationToken)
    {
        context.CurrentStepId = step.Id;

        var result = new StepExecutionResult
        {
            StepId = step.Id,
            StepName = step.Name,
            StartTime = DateTime.UtcNow,
            Status = ExecutionStatus.Running
        };

        try
        {
            if (!step.Enabled)
            {
                _logger.LogInformation("Skipping disabled step: {StepName}", step.Name);
                result.Status = ExecutionStatus.Completed;
                return result;
            }

            if (!_conditionEvaluator.Evaluate(step.Conditions, _variableManager))
            {
                _logger.LogInformation("Skipping step due to conditions not met: {StepName}", step.Name);
                result.Status = ExecutionStatus.Completed;
                return result;
            }

            _logger.LogInformation("Executing step: {StepName} (Type: {StepType})", step.Name, step.Type);

            if (step.Type == StepType.Group)
            {
                foreach (var childStep in step.ChildSteps)
                {
                    var childResult = await ExecuteStepAsync(childStep, context, cancellationToken);
                    context.StepResults.Add(childResult);

                    if (childResult.Status == ExecutionStatus.Failed && !childStep.ContinueOnError)
                    {
                        result.Status = ExecutionStatus.Failed;
                        result.ErrorMessage = $"Child step failed: {childStep.Name}";
                        return result;
                    }
                }
                result.Status = ExecutionStatus.Completed;
            }
            else
            {
                var executor = _stepExecutors.FirstOrDefault(e => e.CanExecute(step));
                if (executor == null)
                {
                    throw new InvalidOperationException($"No executor found for step type: {step.Type}");
                }

                result = await executor.ExecuteAsync(step, context, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Step execution failed: {StepName}", step.Name);
            result.Status = ExecutionStatus.Failed;
            result.ErrorMessage = ex.Message;
        }
        finally
        {
            result.EndTime = DateTime.UtcNow;
        }

        return result;
    }

    public async Task<MDT.Core.Models.ExecutionContext> ExecuteParallelAsync(
        Core.Models.TaskSequence taskSequence,
        int maxParallelism = 4,
        CancellationToken cancellationToken = default)
    {
        var context = new MDT.Core.Models.ExecutionContext
        {
            TaskSequenceId = taskSequence.Id,
            Status = ExecutionStatus.Running,
            StartTime = DateTime.UtcNow
        };

        _logger.LogInformation("Starting parallel task sequence execution: {TaskSequenceName}", taskSequence.Name);

        try
        {
            foreach (var variable in taskSequence.Variables)
            {
                _variableManager.SetVariable(variable.Name, variable.Value);
                context.Variables[variable.Name] = variable.Value;
            }

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxParallelism,
                CancellationToken = cancellationToken
            };

            var results = new System.Collections.Concurrent.ConcurrentBag<StepExecutionResult>();

            await Parallel.ForEachAsync(taskSequence.Steps, parallelOptions, async (step, token) =>
            {
                var result = await ExecuteStepAsync(step, context, token);
                results.Add(result);
            });

            context.StepResults.AddRange(results.OrderBy(r => r.StartTime));

            var hasFailures = context.StepResults.Any(r => r.Status == ExecutionStatus.Failed);
            context.Status = hasFailures ? ExecutionStatus.Failed : ExecutionStatus.Completed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parallel task sequence execution failed");
            context.Status = ExecutionStatus.Failed;
        }
        finally
        {
            context.EndTime = DateTime.UtcNow;
        }

        return context;
    }
}
