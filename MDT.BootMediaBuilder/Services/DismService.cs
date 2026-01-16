using MDT.BootMediaBuilder.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace MDT.BootMediaBuilder.Services;

/// <summary>
/// Service for DISM.exe operations
/// </summary>
public class DismService
{
    private readonly ILogger<DismService> _logger;
    private readonly string _dismPath;

    public DismService(ILogger<DismService> logger, string dismPath)
    {
        _logger = logger;
        _dismPath = dismPath;
    }

    /// <summary>
    /// Mount a WIM image
    /// </summary>
    public async Task MountImageAsync(string wimPath, string mountPath, int imageIndex = 1)
    {
        _logger.LogInformation("Mounting WIM image {WimPath} to {MountPath}", wimPath, mountPath);

        Directory.CreateDirectory(mountPath);

        var args = $"/Mount-Wim /WimFile:\"{wimPath}\" /Index:{imageIndex} /MountDir:\"{mountPath}\"";
        await ExecuteDismCommandAsync(args);

        _logger.LogInformation("Successfully mounted WIM image");
    }

    /// <summary>
    /// Unmount a WIM image
    /// </summary>
    public async Task UnmountImageAsync(string mountPath, bool commit = true)
    {
        _logger.LogInformation("Unmounting image from {MountPath} (commit: {Commit})", mountPath, commit);

        var commitArg = commit ? "/Commit" : "/Discard";
        var args = $"/Unmount-Wim /MountDir:\"{mountPath}\" {commitArg}";
        await ExecuteDismCommandAsync(args);

        _logger.LogInformation("Successfully unmounted image");
    }

    /// <summary>
    /// Add a file or directory to a mounted image
    /// </summary>
    public void InjectFile(string mountPath, string sourcePath, string destinationPath)
    {
        _logger.LogInformation("Injecting {SourcePath} to {DestPath} in mounted image", sourcePath, destinationPath);

        var fullDestPath = Path.Combine(mountPath, destinationPath.TrimStart('\\', '/'));
        var destDir = Path.GetDirectoryName(fullDestPath);

        if (!string.IsNullOrEmpty(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        if (Directory.Exists(sourcePath))
        {
            CopyDirectory(sourcePath, fullDestPath);
        }
        else if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, fullDestPath, true);
        }
        else
        {
            throw new FileNotFoundException($"Source path not found: {sourcePath}");
        }

        _logger.LogInformation("Successfully injected file/directory");
    }

    /// <summary>
    /// Add drivers to a mounted image
    /// </summary>
    public async Task AddDriversAsync(string mountPath, string driverPath, bool recurse = true)
    {
        _logger.LogInformation("Adding drivers from {DriverPath} to mounted image", driverPath);

        if (!Directory.Exists(driverPath))
        {
            throw new DirectoryNotFoundException($"Driver path not found: {driverPath}");
        }

        var recurseArg = recurse ? "/Recurse" : "";
        var args = $"/Image:\"{mountPath}\" /Add-Driver /Driver:\"{driverPath}\" {recurseArg}";
        await ExecuteDismCommandAsync(args);

        _logger.LogInformation("Successfully added drivers");
    }

    /// <summary>
    /// Optimize the image to reduce size
    /// </summary>
    public async Task OptimizeImageAsync(string mountPath)
    {
        _logger.LogInformation("Optimizing image at {MountPath}", mountPath);

        var args = $"/Image:\"{mountPath}\" /Cleanup-Image /StartComponentCleanup /ResetBase";
        await ExecuteDismCommandAsync(args);

        _logger.LogInformation("Successfully optimized image");
    }

    /// <summary>
    /// Cleanup orphaned mount points
    /// </summary>
    public async Task CleanupMountPointsAsync()
    {
        _logger.LogInformation("Cleaning up orphaned mount points");

        var args = "/Cleanup-Wim";
        await ExecuteDismCommandAsync(args);

        _logger.LogInformation("Successfully cleaned up mount points");
    }

    /// <summary>
    /// Execute a DISM command and capture output
    /// </summary>
    private async Task<string> ExecuteDismCommandAsync(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _dismPath,
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
                _logger.LogDebug("DISM Output: {Output}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                errorOutput.AppendLine(e.Data);
                _logger.LogWarning("DISM Error: {Error}", e.Data);
            }
        };

        _logger.LogDebug("Executing DISM: {Path} {Args}", _dismPath, arguments);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        var fullOutput = output.ToString();

        if (process.ExitCode != 0)
        {
            var errorMsg = $"DISM command failed with exit code {process.ExitCode}";
            _logger.LogError("{ErrorMsg}. Output: {Output}", errorMsg, fullOutput);
            throw new DismOperationException(errorMsg, fullOutput);
        }

        return fullOutput;
    }

    /// <summary>
    /// Recursively copy a directory
    /// </summary>
    private void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
            CopyDirectory(subDir, destSubDir);
        }
    }
}
