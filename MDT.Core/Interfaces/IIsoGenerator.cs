namespace MDT.Core.Interfaces;

/// <summary>
/// Service for generating bootable ISO files
/// </summary>
public interface IIsoGenerator
{
    /// <summary>
    /// Generate a bootable ISO file from the specified directory
    /// </summary>
    /// <param name="sourceDirectory">Directory containing boot files</param>
    /// <param name="outputPath">Path where the ISO should be created</param>
    /// <param name="volumeLabel">Volume label for the ISO</param>
    /// <returns>True if ISO was generated successfully</returns>
    Task<bool> GenerateIsoAsync(string sourceDirectory, string outputPath, string volumeLabel);
    
    /// <summary>
    /// Validate that all required boot files are present
    /// </summary>
    /// <param name="directory">Directory to validate</param>
    /// <returns>True if all required files are present</returns>
    bool ValidateBootFiles(string directory);
}
