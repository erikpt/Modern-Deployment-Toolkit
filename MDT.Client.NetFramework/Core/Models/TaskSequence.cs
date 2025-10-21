using System;
using System.Collections.Generic;

namespace MDT.Client.NetFramework.Core.Models
{
    /// <summary>
    /// Represents an MDT task sequence
    /// </summary>
    public class TaskSequence
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<TaskSequenceVariable> Variables { get; set; }
        public List<TaskSequenceStep> Steps { get; set; }

        public TaskSequence()
        {
            Variables = new List<TaskSequenceVariable>();
            Steps = new List<TaskSequenceStep>();
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
            Version = "1.0.0";
        }
    }
}
