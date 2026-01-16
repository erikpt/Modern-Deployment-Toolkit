using System.ComponentModel.DataAnnotations;

namespace MDT.BootMediaBuilder;

/// <summary>
/// Configuration options for the Boot Media Builder service
/// </summary>
public class BootMediaBuilderOptions
{
    /// <summary>
    /// Path to Windows ADK installation
    /// </summary>
    [Required]
    public string AdkPath { get; set; } = @"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit";
    
    /// <summary>
    /// Working directory for temporary build files
    /// </summary>
    [Required]
    public string WorkingDirectory { get; set; } = @"C:\MDT\BootMedia\Working";
    
    /// <summary>
    /// Output directory for completed ISOs
    /// </summary>
    [Required]
    public string OutputDirectory { get; set; } = @"C:\MDT\BootMedia\Output";
    
    /// <summary>
    /// Default architecture if not specified
    /// </summary>
    public string DefaultArchitecture { get; set; } = "amd64";
    
    /// <summary>
    /// Maximum number of concurrent builds allowed
    /// </summary>
    [Range(1, 10)]
    public int MaxConcurrentBuilds { get; set; } = 2;
    
    /// <summary>
    /// Whether to automatically cleanup old build files
    /// </summary>
    public bool AutoCleanup { get; set; } = true;
    
    /// <summary>
    /// Days to retain old ISOs before cleanup
    /// </summary>
    [Range(1, 365)]
    public int CleanupRetentionDays { get; set; } = 30;
    
    /// <summary>
    /// Maximum number of ISOs to retain
    /// </summary>
    [Range(1, 100)]
    public int IsoRetentionCount { get; set; } = 10;
}
