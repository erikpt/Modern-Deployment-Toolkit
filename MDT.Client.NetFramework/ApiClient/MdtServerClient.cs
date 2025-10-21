using System;
using System.IO;
using System.Net;
using System.Text;
using MDT.Client.NetFramework.Core.Interfaces;
using MDT.Client.NetFramework.Core.Models;

namespace MDT.Client.NetFramework.ApiClient
{
    /// <summary>
    /// Client for communicating with MDT.NET 8.0 server
    /// </summary>
    public class MdtServerClient : IServerClient
    {
        private readonly string _serverUrl;
        private readonly string _executionId;

        public MdtServerClient(string serverUrl, string executionId)
        {
            if (string.IsNullOrEmpty(serverUrl))
                throw new ArgumentException("Server URL cannot be null or empty");

            _serverUrl = serverUrl.TrimEnd('/');
            _executionId = executionId ?? Guid.NewGuid().ToString();
        }

        public TaskSequence GetTaskSequence(string taskSequenceId)
        {
            try
            {
                string url = string.Format("{0}/api/tasksequence/{1}", _serverUrl, taskSequenceId);
                string response = HttpGet(url);

                // TODO: Deserialize JSON response to TaskSequence
                // For now, return null as placeholder
                return null;
            }
            catch (Exception ex)
            {
                LogError("Error getting task sequence", ex);
                throw;
            }
        }

        public void ReportProgress(ExecutionContext context)
        {
            try
            {
                string url = string.Format("{0}/api/execution/{1}", _serverUrl, _executionId);
                
                // TODO: Serialize context to JSON and POST to server
                // For now, just log
                Console.WriteLine("Reporting progress to server: {0}", context.Status);
            }
            catch (Exception ex)
            {
                LogError("Error reporting progress", ex);
                // Don't throw - we don't want to fail the task sequence if we can't report
            }
        }

        public void ReportStepComplete(StepExecutionResult result)
        {
            try
            {
                string url = string.Format("{0}/api/execution/{1}/step", _serverUrl, _executionId);
                
                // TODO: Serialize result to JSON and POST to server
                Console.WriteLine("Reporting step complete: {0} - {1}", result.StepName, result.Status);
            }
            catch (Exception ex)
            {
                LogError("Error reporting step completion", ex);
            }
        }

        public void SendLog(string level, string message)
        {
            try
            {
                string url = string.Format("{0}/api/execution/{1}/log", _serverUrl, _executionId);
                
                // TODO: Send log message to server
                Console.WriteLine("[{0}] {1}", level, message);
            }
            catch (Exception ex)
            {
                LogError("Error sending log", ex);
            }
        }

        private string HttpGet(string url)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.Accept] = "application/json";
                return client.DownloadString(url);
            }
        }

        private string HttpPost(string url, string jsonData)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.Accept] = "application/json";
                return client.UploadString(url, "POST", jsonData);
            }
        }

        private void LogError(string message, Exception ex)
        {
            Console.WriteLine("ERROR: {0} - {1}", message, ex.Message);
        }
    }
}
