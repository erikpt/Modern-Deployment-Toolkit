namespace MDT.Core.Models;

public class TaskSequence
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    public List<TaskSequenceVariable> Variables { get; set; } = new();
    public List<TaskSequenceStep> Steps { get; set; } = new();
}
