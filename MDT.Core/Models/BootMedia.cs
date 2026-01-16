namespace MDT.Core.Models;

/// <summary>
/// Represents a generated boot media ISO file
/// </summary>
public class BootMedia
{
    /// <summary>
    /// Unique identifier for this boot media
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// File name of the generated ISO
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Full path to the ISO file
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Size of the ISO file in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Target architecture (amd64 or x86)
    /// </summary>
    public string Architecture { get; set; } = string.Empty;
    
    /// <summary>
    /// MDT server URL configured in the boot media
    /// </summary>
    public string ServerUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Date when the boot media was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Current status of the boot media
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// List of driver paths that were included
    /// </summary>
    public string IncludedDrivers { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional components included (PowerShell, WMI, etc.)
    /// </summary>
    public string OptionalComponents { get; set; } = string.Empty;
    
    /// <summary>
    /// Build log and notes
    /// </summary>
    public string BuildLog { get; set; } = string.Empty;
}
