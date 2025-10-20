using System;
using System.Collections.Generic;

namespace MDT.Client.NetFramework.Core.Models
{
    /// <summary>
    /// Represents a step in a task sequence
    /// </summary>
    public class TaskSequenceStep
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public bool Enabled { get; set; }
        public bool ContinueOnError { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public List<TaskSequenceStep> ChildSteps { get; set; }
        public List<TaskSequenceCondition> Conditions { get; set; }

        public TaskSequenceStep()
        {
            Enabled = true;
            Properties = new Dictionary<string, string>();
            ChildSteps = new List<TaskSequenceStep>();
            Conditions = new List<TaskSequenceCondition>();
        }
    }

    /// <summary>
    /// Represents a condition for step execution
    /// </summary>
    public class TaskSequenceCondition
    {
        public string Type { get; set; }
        public string Expression { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public TaskSequenceCondition()
        {
            Properties = new Dictionary<string, string>();
        }
    }
}
