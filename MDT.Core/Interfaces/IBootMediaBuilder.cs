using MDT.Core.Models;

namespace MDT.Core.Interfaces;

/// <summary>
/// Service for building bootable Windows PE media
/// </summary>
public interface IBootMediaBuilder
{
    /// <summary>
    /// Queue a new boot media build
    /// </summary>
    /// <param name="options">Build configuration options</param>
    /// <returns>Build operation tracking object</returns>
    Task<BootMediaBuild> BuildAsync(BootMediaBuildOptions options);
    
    /// <summary>
    /// Get the status of a build operation
    /// </summary>
    /// <param name="buildId">Build ID to check</param>
    /// <returns>Current build status</returns>
    Task<BootMediaBuild?> GetStatusAsync(string buildId);
    
    /// <summary>
    /// Get all available boot media
    /// </summary>
    /// <returns>List of all boot media</returns>
    Task<List<BootMedia>> GetAllAsync();
    
    /// <summary>
    /// Delete a boot media and its ISO file
    /// </summary>
    /// <param name="buildId">Build ID or boot media ID to delete</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAsync(string buildId);
}
