using MDT.Client.NetFramework.Core.Models;

namespace MDT.Client.NetFramework.Core.Interfaces
{
    /// <summary>
    /// Interface for communicating with MDT server
    /// </summary>
    public interface IServerClient
    {
        /// <summary>
        /// Gets a task sequence from the server
        /// </summary>
        TaskSequence GetTaskSequence(string taskSequenceId);

        /// <summary>
        /// Reports progress to the server
        /// </summary>
        void ReportProgress(ExecutionContext context);

        /// <summary>
        /// Reports step completion to the server
        /// </summary>
        void ReportStepComplete(StepExecutionResult result);

        /// <summary>
        /// Sends a log message to the server
        /// </summary>
        void SendLog(string level, string message);
    }
}
