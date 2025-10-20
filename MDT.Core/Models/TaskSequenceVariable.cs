namespace MDT.Core.Models;

public class TaskSequenceVariable
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsReadOnly { get; set; }
    public bool IsSecret { get; set; }
}
