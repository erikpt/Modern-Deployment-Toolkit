using MDT.Core.Interfaces;
using MDT.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDT.Plugins.Steps;

public class SetVariableExecutor : BaseStepExecutor
{
    private readonly IVariableManager _variableManager;

    public SetVariableExecutor(IVariableManager variableManager, ILogger<SetVariableExecutor> logger) : base(logger)
    {
        _variableManager = variableManager;
    }

    public override StepType SupportedStepType => StepType.SetVariable;

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
            var variableName = step.Properties.GetValueOrDefault("VariableName", "");
            var variableValue = step.Properties.GetValueOrDefault("VariableValue", "");

            if (string.IsNullOrEmpty(variableName))
            {
                throw new InvalidOperationException("VariableName property is required");
            }

            Logger.LogInformation("Setting variable {VariableName} = {VariableValue}", variableName, variableValue);

            _variableManager.SetVariable(variableName, variableValue);
            context.Variables[variableName] = variableValue;

            await Task.CompletedTask;

            result.Status = ExecutionStatus.Completed;
            result.OutputVariables[variableName] = variableValue;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to set variable");
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
