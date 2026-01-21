namespace MDT.Core.Models;

/// <summary>
/// Represents an in-progress or completed boot media build operation
/// </summary>
public class BootMediaBuild
{
    /// <summary>
    /// Unique identifier for this build operation
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Current status of the build
    /// </summary>
    public BootMediaBuildStatus Status { get; set; } = BootMediaBuildStatus.Queued;
    
    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int Progress { get; set; }
    
    /// <summary>
    /// Description of the current build step
    /// </summary>
    public string CurrentStep { get; set; } = string.Empty;
    
    /// <summary>
    /// When the build started
    /// </summary>
    public DateTime? StartTime { get; set; }
    
    /// <summary>
    /// When the build completed or failed
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// Error message if the build failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Serialized build options used for this build
    /// </summary>
    public string BuildOptions { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the resulting BootMedia if build completed successfully
    /// </summary>
    public string? ResultingBootMediaId { get; set; }
}
