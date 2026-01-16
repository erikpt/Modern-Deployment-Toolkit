namespace MDT.Core.Models;

/// <summary>
/// Represents the status of a boot media build operation
/// </summary>
public enum BootMediaBuildStatus
{
    /// <summary>
    /// Build has been queued but not started
    /// </summary>
    Queued,
    
    /// <summary>
    /// Build is currently in progress
    /// </summary>
    Building,
    
    /// <summary>
    /// Build completed successfully
    /// </summary>
    Completed,
    
    /// <summary>
    /// Build failed with errors
    /// </summary>
    Failed
}
