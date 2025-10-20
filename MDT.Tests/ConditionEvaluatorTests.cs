using MDT.Core.Models;
using MDT.Core.Services;
using Xunit;

namespace MDT.Tests;

public class ConditionEvaluatorTests
{
    [Fact]
    public void Evaluate_Equals_ShouldReturnTrue()
    {
        var evaluator = new ConditionEvaluator();
        var manager = new VariableManager();
        manager.SetVariable("OSVersion", "Windows11");

        var conditions = new List<TaskSequenceCondition>
        {
            new() { VariableName = "OSVersion", Operator = ConditionOperator.Equals, Value = "Windows11" }
        };

        var result = evaluator.Evaluate(conditions, manager);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_NotEquals_ShouldReturnTrue()
    {
        var evaluator = new ConditionEvaluator();
        var manager = new VariableManager();
        manager.SetVariable("OSVersion", "Windows11");

        var conditions = new List<TaskSequenceCondition>
        {
            new() { VariableName = "OSVersion", Operator = ConditionOperator.NotEquals, Value = "Windows10" }
        };

        var result = evaluator.Evaluate(conditions, manager);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_Contains_ShouldReturnTrue()
    {
        var evaluator = new ConditionEvaluator();
        var manager = new VariableManager();
        manager.SetVariable("ComputerName", "PC001-LAB");

        var conditions = new List<TaskSequenceCondition>
        {
            new() { VariableName = "ComputerName", Operator = ConditionOperator.Contains, Value = "LAB" }
        };

        var result = evaluator.Evaluate(conditions, manager);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_GreaterThan_ShouldReturnTrue()
    {
        var evaluator = new ConditionEvaluator();
        var manager = new VariableManager();
        manager.SetVariable("Memory", "16");

        var conditions = new List<TaskSequenceCondition>
        {
            new() { VariableName = "Memory", Operator = ConditionOperator.GreaterThan, Value = "8" }
        };

        var result = evaluator.Evaluate(conditions, manager);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_EmptyConditions_ShouldReturnTrue()
    {
        var evaluator = new ConditionEvaluator();
        var manager = new VariableManager();

        var result = evaluator.Evaluate(new List<TaskSequenceCondition>(), manager);
        Assert.True(result);
    }
}
