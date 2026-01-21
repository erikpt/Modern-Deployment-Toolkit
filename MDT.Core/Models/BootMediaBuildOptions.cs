namespace MDT.Core.Models;

/// <summary>
/// Configuration options for building boot media
/// </summary>
public class BootMediaBuildOptions
{
    /// <summary>
    /// Target architecture (amd64 or x86)
    /// </summary>
    public string Architecture { get; set; } = "amd64";
    
    /// <summary>
    /// MDT server URL that the boot media will connect to
    /// </summary>
    public string ServerUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to include additional network drivers
    /// </summary>
    public bool IncludeDrivers { get; set; }
    
    /// <summary>
    /// Paths to driver folders to inject (if IncludeDrivers is true)
    /// </summary>
    public List<string> DriverPaths { get; set; } = new();
    
    /// <summary>
    /// Whether to include PowerShell support in WinPE
    /// </summary>
    public bool IncludePowerShell { get; set; } = true;
    
    /// <summary>
    /// Whether to include WMI support in WinPE
    /// </summary>
    public bool IncludeWmi { get; set; } = true;
    
    /// <summary>
    /// Whether to optimize the image (reduce size)
    /// </summary>
    public bool OptimizeImage { get; set; } = true;
}
