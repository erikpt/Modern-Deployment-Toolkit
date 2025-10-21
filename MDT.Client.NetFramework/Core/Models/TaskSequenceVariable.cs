using System;

namespace MDT.Client.NetFramework.Core.Models
{
    /// <summary>
    /// Represents a task sequence variable
    /// </summary>
    public class TaskSequenceVariable
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsSecret { get; set; }

        public TaskSequenceVariable()
        {
            IsReadOnly = false;
            IsSecret = false;
        }
    }
}
