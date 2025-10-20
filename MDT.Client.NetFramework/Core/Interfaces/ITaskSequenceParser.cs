using MDT.Client.NetFramework.Core.Models;

namespace MDT.Client.NetFramework.Core.Interfaces
{
    /// <summary>
    /// Interface for task sequence parsers
    /// </summary>
    public interface ITaskSequenceParser
    {
        /// <summary>
        /// Parses a task sequence from content
        /// </summary>
        TaskSequence Parse(string content);

        /// <summary>
        /// Determines if the parser can parse the given content
        /// </summary>
        bool CanParse(string content);
    }
}
