using System;
using System.Collections.Generic;

namespace MDT.Client.NetFramework.Core.Models
{
    /// <summary>
    /// Execution status enumeration
    /// </summary>
    public enum ExecutionStatus
    {
        NotStarted,
        Running,
        Completed,
        Failed,
        Cancelled
    }

    /// <summary>
    /// Represents the execution context for a task sequence
    /// </summary>
    public class ExecutionContext
    {
        public string ExecutionId { get; set; }
        public string TaskSequenceId { get; set; }
        public ExecutionStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public Dictionary<string, string> Variables { get; set; }
        public List<StepExecutionResult> StepResults { get; set; }
        public string CurrentStepId { get; set; }
        public string ServerUrl { get; set; }

        public ExecutionContext()
        {
            ExecutionId = Guid.NewGuid().ToString();
            Status = ExecutionStatus.NotStarted;
            Variables = new Dictionary<string, string>();
            StepResults = new List<StepExecutionResult>();
        }
    }

    /// <summary>
    /// Represents the result of a step execution
    /// </summary>
    public class StepExecutionResult
    {
        public string StepId { get; set; }
        public string StepName { get; set; }
        public ExecutionStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ErrorMessage { get; set; }
        public int ExitCode { get; set; }
        public Dictionary<string, string> OutputVariables { get; set; }

        public StepExecutionResult()
        {
            OutputVariables = new Dictionary<string, string>();
        }
    }
}
