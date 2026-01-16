namespace MDT.Core.Interfaces;

/// <summary>
/// Service for interacting with Windows Assessment and Deployment Kit (ADK)
/// </summary>
public interface IAdkService
{
    /// <summary>
    /// Validate that ADK is installed and accessible
    /// </summary>
    /// <returns>True if ADK is properly installed</returns>
    bool ValidateInstallation();
    
    /// <summary>
    /// Get the path to the WinPE files for the specified architecture
    /// </summary>
    /// <param name="architecture">Target architecture (amd64 or x86)</param>
    /// <returns>Path to WinPE files</returns>
    string GetWinPEPath(string architecture);
    
    /// <summary>
    /// Get the path to deployment tools (DISM, oscdimg, etc.)
    /// </summary>
    /// <returns>Path to deployment tools directory</returns>
    string GetDeploymentToolsPath();
}
