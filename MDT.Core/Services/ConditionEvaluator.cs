using MDT.Core.Interfaces;
using MDT.Core.Models;

namespace MDT.Core.Services;

public class ConditionEvaluator : IConditionEvaluator
{
    public bool Evaluate(List<TaskSequenceCondition> conditions, IVariableManager variableManager)
    {
        if (conditions == null || conditions.Count == 0)
            return true;

        return conditions.All(condition => EvaluateCondition(condition, variableManager));
    }

    public bool EvaluateCondition(TaskSequenceCondition condition, IVariableManager variableManager)
    {
        var variableValue = variableManager.GetVariable(condition.VariableName);

        return condition.Operator switch
        {
            ConditionOperator.Equals => string.Equals(variableValue, condition.Value, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.NotEquals => !string.Equals(variableValue, condition.Value, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.GreaterThan => CompareNumeric(variableValue, condition.Value) > 0,
            ConditionOperator.LessThan => CompareNumeric(variableValue, condition.Value) < 0,
            ConditionOperator.Contains => variableValue.Contains(condition.Value, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.Exists => variableManager.VariableExists(condition.VariableName),
            _ => false
        };
    }

    private static int CompareNumeric(string value1, string value2)
    {
        if (double.TryParse(value1, out var num1) && double.TryParse(value2, out var num2))
        {
            return num1.CompareTo(num2);
        }
        return string.Compare(value1, value2, StringComparison.OrdinalIgnoreCase);
    }
}
