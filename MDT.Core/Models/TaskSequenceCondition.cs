namespace MDT.Core.Models;

public enum ConditionOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    Contains,
    Exists
}

public class TaskSequenceCondition
{
    public string VariableName { get; set; } = string.Empty;
    public ConditionOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
}
