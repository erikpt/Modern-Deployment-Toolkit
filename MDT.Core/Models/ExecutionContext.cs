namespace MDT.Core.Models;

public enum ExecutionStatus
{
    NotStarted,
    Running,
    Completed,
    Failed,
    Cancelled
}

public class ExecutionContext
{
    public string ExecutionId { get; set; } = Guid.NewGuid().ToString();
    public string TaskSequenceId { get; set; } = string.Empty;
    public ExecutionStatus Status { get; set; } = ExecutionStatus.NotStarted;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public Dictionary<string, string> Variables { get; set; } = new();
    public List<StepExecutionResult> StepResults { get; set; } = new();
    public string CurrentStepId { get; set; } = string.Empty;
}

public class StepExecutionResult
{
    public string StepId { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public ExecutionStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public Dictionary<string, string> OutputVariables { get; set; } = new();
}
