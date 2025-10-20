using MDT.Core.Models;

namespace MDT.Core.Interfaces;

public interface IConditionEvaluator
{
    bool Evaluate(List<TaskSequenceCondition> conditions, IVariableManager variableManager);
    bool EvaluateCondition(TaskSequenceCondition condition, IVariableManager variableManager);
}
