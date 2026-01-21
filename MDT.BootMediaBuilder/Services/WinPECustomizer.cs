using Microsoft.Extensions.Logging;
using System.Text;

namespace MDT.BootMediaBuilder.Services;

/// <summary>
/// Service for customizing WinPE images
/// </summary>
public class WinPECustomizer
{
    private readonly ILogger<WinPECustomizer> _logger;
    private readonly string _templatesPath;

    public WinPECustomizer(ILogger<WinPECustomizer> logger, string templatesPath)
    {
        _logger = logger;
        _templatesPath = templatesPath;
    }

    /// <summary>
    /// Create the startup script from template
    /// </summary>
    public void CreateStartupScript(string mountPath, string serverUrl)
    {
        _logger.LogInformation("Creating startup script in mounted image");

        var templatePath = Path.Combine(_templatesPath, "startnet.cmd.template");
        if (!File.Exists(templatePath))
        {
            _logger.LogWarning("Template not found, creating default startnet.cmd");
            var defaultContent = CreateDefaultStartnetCmd();
            var targetPath = Path.Combine(mountPath, "Windows", "System32", "startnet.cmd");
            File.WriteAllText(targetPath, defaultContent, Encoding.ASCII);
            return;
        }

        var content = File.ReadAllText(templatePath);
        content = content.Replace("{{SERVER_URL}}", serverUrl);

        var startnetPath = Path.Combine(mountPath, "Windows", "System32", "startnet.cmd");
        File.WriteAllText(startnetPath, content, Encoding.ASCII);

        _logger.LogInformation("Created startnet.cmd at {Path}", startnetPath);
    }

    /// <summary>
    /// Create configuration file for MDT client
    /// </summary>
    public void CreateConfigFile(string mountPath, string serverUrl)
    {
        _logger.LogInformation("Creating config.ini for MDT client");

        var mdtDir = Path.Combine(mountPath, "MDT");
        Directory.CreateDirectory(mdtDir);

        var templatePath = Path.Combine(_templatesPath, "config.ini.template");
        string content;

        if (File.Exists(templatePath))
        {
            content = File.ReadAllText(templatePath);
            content = content.Replace("{{SERVER_URL}}", serverUrl);
        }
        else
        {
            _logger.LogWarning("Template not found, creating default config.ini");
            content = CreateDefaultConfigIni(serverUrl);
        }

        var configPath = Path.Combine(mdtDir, "config.ini");
        File.WriteAllText(configPath, content, Encoding.ASCII);

        _logger.LogInformation("Created config.ini at {Path}", configPath);
    }

    /// <summary>
    /// Create unattend.xml for WinPE automation
    /// </summary>
    public void CreateUnattendXml(string mountPath, string architecture)
    {
        _logger.LogInformation("Creating unattend.xml for architecture {Architecture}", architecture);

        var templatePath = Path.Combine(_templatesPath, "unattend.xml.template");
        string content;

        if (File.Exists(templatePath))
        {
            content = File.ReadAllText(templatePath);
            content = content.Replace("{{ARCHITECTURE}}", architecture);
        }
        else
        {
            _logger.LogWarning("Template not found, creating default unattend.xml");
            content = CreateDefaultUnattendXml(architecture);
        }

        var unattendPath = Path.Combine(mountPath, "Windows", "System32", "unattend.xml");
        File.WriteAllText(unattendPath, content, Encoding.UTF8);

        _logger.LogInformation("Created unattend.xml at {Path}", unattendPath);
    }

    /// <summary>
    /// Inject MDT client executable into the image
    /// </summary>
    public void InjectMdtClient(string mountPath, string mdtClientPath)
    {
        _logger.LogInformation("Injecting MDT client from {ClientPath}", mdtClientPath);

        if (!File.Exists(mdtClientPath))
        {
            throw new FileNotFoundException($"MDT client not found at: {mdtClientPath}");
        }

        var mdtDir = Path.Combine(mountPath, "MDT");
        Directory.CreateDirectory(mdtDir);

        var targetPath = Path.Combine(mdtDir, "MDT.Client.exe");
        File.Copy(mdtClientPath, targetPath, true);

        // Create logs directory
        var logsDir = Path.Combine(mdtDir, "Logs");
        Directory.CreateDirectory(logsDir);

        _logger.LogInformation("MDT client injected successfully");
    }

    private string CreateDefaultStartnetCmd()
    {
        return @"@echo off
echo Starting Modern Deployment Toolkit Client...
echo.

REM Wait for network initialization
wpeinit

REM Configure network
wpeutil InitializeNetwork

REM Wait for network to be ready
ping -n 10 127.0.0.1 > nul

REM Start MDT Client
X:\MDT\MDT.Client.exe /config:X:\MDT\config.ini

REM If MDT Client exits, drop to command prompt
echo.
echo MDT Client exited. Dropping to command prompt...
cmd.exe
";
    }

    private string CreateDefaultConfigIni(string serverUrl)
    {
        return $@"[Settings]
ServerUrl={serverUrl}
AutoConnect=true
LogLevel=Information
LogPath=X:\MDT\Logs

[Network]
EnableDHCP=true
WaitForNetwork=30

[TaskSequence]
AutoSelectProduction=true
ShowOnlyProduction=true
";
    }

    private string CreateDefaultUnattendXml(string architecture)
    {
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<unattend xmlns=""urn:schemas-microsoft-com:unattend"">
    <settings pass=""windowsPE"">
        <component name=""Microsoft-Windows-Setup"" processorArchitecture=""{architecture}"" publicKeyToken=""31bf3856ad364e35"" language=""neutral"" versionScope=""nonSxS"" xmlns:wcm=""http://schemas.microsoft.com/WMIConfig/2002/State"">
            <Display>
                <ColorDepth>32</ColorDepth>
                <HorizontalResolution>1024</HorizontalResolution>
                <VerticalResolution>768</VerticalResolution>
                <RefreshRate>60</RefreshRate>
            </Display>
        </component>
        <component name=""Microsoft-Windows-International-Core-WinPE"" processorArchitecture=""{architecture}"" publicKeyToken=""31bf3856ad364e35"" language=""neutral"" versionScope=""nonSxS"" xmlns:wcm=""http://schemas.microsoft.com/WMIConfig/2002/State"">
            <SetupUILanguage>
                <UILanguage>en-US</UILanguage>
            </SetupUILanguage>
            <InputLocale>en-US</InputLocale>
            <SystemLocale>en-US</SystemLocale>
            <UILanguage>en-US</UILanguage>
            <UserLocale>en-US</UserLocale>
        </component>
    </settings>
</unattend>
";
    }
}
