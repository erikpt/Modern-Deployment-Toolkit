using MDT.Core.Interfaces;
using MDT.BootMediaBuilder.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MDT.BootMediaBuilder.Services;

/// <summary>
/// Service for interacting with Windows Assessment and Deployment Kit (ADK)
/// </summary>
public class AdkService : IAdkService
{
    private readonly ILogger<AdkService> _logger;
    private readonly BootMediaBuilderOptions _options;

    public AdkService(ILogger<AdkService> logger, IOptions<BootMediaBuilderOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Validate that ADK is installed and accessible
    /// </summary>
    public bool ValidateInstallation()
    {
        try
        {
            _logger.LogInformation("Validating ADK installation at {AdkPath}", _options.AdkPath);

            if (!Directory.Exists(_options.AdkPath))
            {
                _logger.LogError("ADK path does not exist: {AdkPath}", _options.AdkPath);
                return false;
            }

            // Check for deployment tools
            var deploymentToolsPath = GetDeploymentToolsPath();
            if (!Directory.Exists(deploymentToolsPath))
            {
                _logger.LogError("Deployment tools path does not exist: {Path}", deploymentToolsPath);
                return false;
            }

            // Check for DISM
            var dismPath = Path.Combine(deploymentToolsPath, "DISM", "dism.exe");
            if (!File.Exists(dismPath))
            {
                _logger.LogError("DISM not found at: {Path}", dismPath);
                return false;
            }

            // Check for oscdimg
            var oscdimgPath = Path.Combine(deploymentToolsPath, "Oscdimg", "oscdimg.exe");
            if (!File.Exists(oscdimgPath))
            {
                _logger.LogError("oscdimg not found at: {Path}", oscdimgPath);
                return false;
            }

            // Check for WinPE
            var winPePath = Path.Combine(_options.AdkPath, "Windows Preinstallation Environment");
            if (!Directory.Exists(winPePath))
            {
                _logger.LogError("WinPE path does not exist: {Path}", winPePath);
                return false;
            }

            _logger.LogInformation("ADK installation validated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating ADK installation");
            return false;
        }
    }

    /// <summary>
    /// Get the path to the WinPE files for the specified architecture
    /// </summary>
    public string GetWinPEPath(string architecture)
    {
        var winPeBasePath = Path.Combine(_options.AdkPath, "Windows Preinstallation Environment");
        var winPePath = Path.Combine(winPeBasePath, architecture);

        if (!Directory.Exists(winPePath))
        {
            throw new AdkNotFoundException($"WinPE path not found for architecture {architecture}: {winPePath}");
        }

        _logger.LogInformation("WinPE path for {Architecture}: {Path}", architecture, winPePath);
        return winPePath;
    }

    /// <summary>
    /// Get the path to deployment tools (DISM, oscdimg, etc.)
    /// </summary>
    public string GetDeploymentToolsPath()
    {
        var deploymentToolsPath = Path.Combine(_options.AdkPath, "Deployment Tools");

        if (!Directory.Exists(deploymentToolsPath))
        {
            throw new AdkNotFoundException($"Deployment tools path not found: {deploymentToolsPath}");
        }

        return deploymentToolsPath;
    }
}
