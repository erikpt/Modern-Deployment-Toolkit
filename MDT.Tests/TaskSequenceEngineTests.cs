using MDT.Core.Interfaces;
using MDT.Core.Models;
using MDT.Core.Services;
using MDT.TaskSequence.Executors;
using MDT.Plugins.Steps;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MDT.Tests;

public class TaskSequenceEngineTests
{
    [Fact]
    public async Task ExecuteAsync_SimpleTaskSequence_ShouldComplete()
    {
        var mockLogger = new Mock<ILogger<TaskSequenceEngine>>();
        var mockStepLogger = new Mock<ILogger<SetVariableExecutor>>();
        var variableManager = new VariableManager();
        var conditionEvaluator = new ConditionEvaluator();

        var stepExecutors = new List<IStepExecutor>
        {
            new SetVariableExecutor(variableManager, mockStepLogger.Object)
        };

        var engine = new TaskSequenceEngine(
            stepExecutors,
            variableManager,
            conditionEvaluator,
            mockLogger.Object);

        var taskSequence = new Core.Models.TaskSequence
        {
            Name = "Test Sequence",
            Steps = new List<TaskSequenceStep>
            {
                new()
                {
                    Name = "Set Variable",
                    Type = StepType.SetVariable,
                    Properties = new Dictionary<string, string>
                    {
                        { "VariableName", "TestVar" },
                        { "VariableValue", "TestValue" }
                    }
                }
            }
        };

        var result = await engine.ExecuteAsync(taskSequence);

        Assert.Equal(ExecutionStatus.Completed, result.Status);
        Assert.Single(result.StepResults);
        Assert.Equal(ExecutionStatus.Completed, result.StepResults[0].Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithDisabledStep_ShouldSkip()
    {
        var mockLogger = new Mock<ILogger<TaskSequenceEngine>>();
        var variableManager = new VariableManager();
        var conditionEvaluator = new ConditionEvaluator();

        var engine = new TaskSequenceEngine(
            new List<IStepExecutor>(),
            variableManager,
            conditionEvaluator,
            mockLogger.Object);

        var taskSequence = new Core.Models.TaskSequence
        {
            Name = "Test Sequence",
            Steps = new List<TaskSequenceStep>
            {
                new()
                {
                    Name = "Disabled Step",
                    Type = StepType.Custom,
                    Enabled = false
                }
            }
        };

        var result = await engine.ExecuteAsync(taskSequence);

        Assert.Equal(ExecutionStatus.Completed, result.Status);
        Assert.Single(result.StepResults);
        Assert.Equal(ExecutionStatus.Completed, result.StepResults[0].Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingCondition_ShouldSkipStep()
    {
        var mockLogger = new Mock<ILogger<TaskSequenceEngine>>();
        var variableManager = new VariableManager();
        variableManager.SetVariable("OSVersion", "Windows10");
        
        var conditionEvaluator = new ConditionEvaluator();

        var engine = new TaskSequenceEngine(
            new List<IStepExecutor>(),
            variableManager,
            conditionEvaluator,
            mockLogger.Object);

        var taskSequence = new Core.Models.TaskSequence
        {
            Name = "Test Sequence",
            Steps = new List<TaskSequenceStep>
            {
                new()
                {
                    Name = "Conditional Step",
                    Type = StepType.Custom,
                    Conditions = new List<TaskSequenceCondition>
                    {
                        new()
                        {
                            VariableName = "OSVersion",
                            Operator = ConditionOperator.Equals,
                            Value = "Windows11"
                        }
                    }
                }
            }
        };

        var result = await engine.ExecuteAsync(taskSequence);

        Assert.Equal(ExecutionStatus.Completed, result.Status);
        Assert.Single(result.StepResults);
    }
}
