using MDT.Core.Interfaces;
using MDT.BootMediaBuilder.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace MDT.BootMediaBuilder.Services;

/// <summary>
/// Service for generating bootable ISO files
/// </summary>
public class IsoGeneratorService : IIsoGenerator
{
    private readonly ILogger<IsoGeneratorService> _logger;
    private readonly string _oscdimgPath;

    public IsoGeneratorService(ILogger<IsoGeneratorService> logger, string oscdimgPath)
    {
        _logger = logger;
        _oscdimgPath = oscdimgPath;
    }

    /// <summary>
    /// Generate a bootable ISO file from the specified directory
    /// </summary>
    public async Task<bool> GenerateIsoAsync(string sourceDirectory, string outputPath, string volumeLabel)
    {
        _logger.LogInformation("Generating ISO from {SourceDir} to {OutputPath}", sourceDirectory, outputPath);

        if (!ValidateBootFiles(sourceDirectory))
        {
            throw new BuildFailedException("Required boot files are missing from source directory");
        }

        // Ensure output directory exists
        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        // Delete existing ISO if present
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        // Build oscdimg arguments for dual BIOS/UEFI boot
        var bootDir = Path.Combine(sourceDirectory, "boot");
        var efiBootDir = Path.Combine(sourceDirectory, "efi", "microsoft", "boot");
        
        var etfsbootPath = Path.Combine(bootDir, "etfsboot.com");
        var efisysPath = Path.Combine(efiBootDir, "efisys.bin");

        // oscdimg arguments for hybrid BIOS/UEFI boot
        var arguments = $"-m -o -u2 -udfver102 -bootdata:2#p0,e,b\"{etfsbootPath}\"#pEF,e,b\"{efisysPath}\" -l\"{volumeLabel}\" \"{sourceDirectory}\" \"{outputPath}\"";

        _logger.LogDebug("oscdimg arguments: {Args}", arguments);

        var startInfo = new ProcessStartInfo
        {
            FileName = _oscdimgPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var output = new StringBuilder();
        var errorOutput = new StringBuilder();

        using var process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
                _logger.LogDebug("oscdimg Output: {Output}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                errorOutput.AppendLine(e.Data);
                _logger.LogWarning("oscdimg Error: {Error}", e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var errorMsg = $"ISO generation failed with exit code {process.ExitCode}";
            _logger.LogError("{ErrorMsg}. Output: {Output}", errorMsg, output.ToString());
            throw new BuildFailedException(errorMsg);
        }

        if (!File.Exists(outputPath))
        {
            throw new BuildFailedException("ISO file was not created");
        }

        var isoSize = new FileInfo(outputPath).Length;
        _logger.LogInformation("Successfully generated ISO: {OutputPath} ({Size} bytes)", outputPath, isoSize);

        return true;
    }

    /// <summary>
    /// Validate that all required boot files are present
    /// </summary>
    public bool ValidateBootFiles(string directory)
    {
        _logger.LogInformation("Validating boot files in {Directory}", directory);

        var requiredFiles = new[]
        {
            Path.Combine("boot", "bcd"),
            Path.Combine("boot", "boot.sdi"),
            Path.Combine("boot", "etfsboot.com"),
            Path.Combine("sources", "boot.wim"),
            Path.Combine("efi", "microsoft", "boot", "efisys.bin"),
            Path.Combine("efi", "boot", "bootx64.efi")
        };

        var missingFiles = new List<string>();

        foreach (var file in requiredFiles)
        {
            var fullPath = Path.Combine(directory, file);
            if (!File.Exists(fullPath))
            {
                missingFiles.Add(file);
                _logger.LogWarning("Required boot file missing: {File}", file);
            }
        }

        if (missingFiles.Count > 0)
        {
            _logger.LogError("Boot file validation failed. Missing {Count} files", missingFiles.Count);
            return false;
        }

        _logger.LogInformation("All required boot files are present");
        return true;
    }
}
