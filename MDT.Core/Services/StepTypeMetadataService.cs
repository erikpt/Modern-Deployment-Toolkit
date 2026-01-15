using MDT.Core.Models;

namespace MDT.Core.Services;

public class StepTypeMetadataService
{
    public List<StepTypeMetadata> GetAllStepTypes()
    {
        return new List<StepTypeMetadata>
        {
            new StepTypeMetadata
            {
                Type = "Group",
                DisplayName = "Group",
                Description = "Organize steps into a logical group",
                Icon = "folder",
                Properties = new List<PropertyDefinition>()
            },
            new StepTypeMetadata
            {
                Type = "InstallOperatingSystem",
                DisplayName = "Install Operating System",
                Description = "Install the operating system",
                Icon = "desktop",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition { Name = "ImagePath", Type = "string", Required = true, Description = "Path to OS image" },
                    new PropertyDefinition { Name = "ImageIndex", Type = "string", Required = true, Description = "Image index to install" }
                }
            },
            new StepTypeMetadata
            {
                Type = "ApplyWindowsImage",
                DisplayName = "Apply Windows Image",
                Description = "Apply a Windows WIM image to a drive",
                Icon = "image",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition { Name = "WimPath", Type = "string", Required = true, Description = "Path to WIM file" },
                    new PropertyDefinition { Name = "ImageIndex", Type = "string", Required = true, Description = "Image index in WIM" },
                    new PropertyDefinition { Name = "TargetDrive", Type = "string", Required = true, Description = "Target drive letter (e.g., C:)" }
                }
            },
            new StepTypeMetadata
            {
                Type = "ApplyFFUImage",
                DisplayName = "Apply FFU Image",
                Description = "Apply a Full Flash Update (FFU) image",
                Icon = "flash",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition { Name = "FFUPath", Type = "string", Required = true, Description = "Path to FFU file" },
                    new PropertyDefinition { Name = "TargetDisk", Type = "string", Required = true, Description = "Target disk number" }
                }
            },
            new StepTypeMetadata
            {
                Type = "InstallApplication",
                DisplayName = "Install Application",
                Description = "Install an application",
                Icon = "package",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition { Name = "ApplicationName", Type = "string", Required = true, Description = "Name of application" },
                    new PropertyDefinition { Name = "InstallCommand", Type = "string", Required = true, Description = "Installation command" },
                    new PropertyDefinition { Name = "SourcePath", Type = "string", Required = false, Description = "Source path for application files" }
                }
            },
            new StepTypeMetadata
            {
                Type = "InstallDriver",
                DisplayName = "Install Driver",
                Description = "Install device drivers",
                Icon = "chip",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition { Name = "DriverPath", Type = "string", Required = true, Description = "Path to driver files or INF" },
                    new PropertyDefinition { Name = "Recurse", Type = "bool", Required = false, DefaultValue = "true", Description = "Recurse subdirectories" }
                }
            },
            new StepTypeMetadata
            {
                Type = "CaptureUserState",
                DisplayName = "Capture User State",
                Description = "Capture user data using USMT",
                Icon = "download",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition { Name = "StorePath", Type = "string", Required = true, Description = "Path to store captured data" },
                    new PropertyDefinition { Name = "USMTPackage", Type = "string", Required = false, Description = "Path to USMT package" }
                }
            },
            new StepTypeMetadata
            {
                Type = "RestoreUserState",
                DisplayName = "Restore User State",
                Description = "Restore user data using USMT",
                Icon = "upload",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition { Name = "StorePath", Type = "string", Required = true, Description = "Path to captured data" },
                    new PropertyDefinition { Name = "USMTPackage", Type = "string", Required = false, Description = "Path to USMT package" }
                }
            },
            new StepTypeMetadata
            {
                Type = "RunCommandLine",
                DisplayName = "Run Command Line",
                Description = "Execute a command line",
                Icon = "terminal",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition { Name = "CommandLine", Type = "string", Required = true, Description = "Command to execute" },
                    new PropertyDefinition { Name = "WorkingDirectory", Type = "string", Required = false, Description = "Working directory" },
                    new PropertyDefinition { Name = "Timeout", Type = "int", Required = false, DefaultValue = "3600", Description = "Timeout in seconds" }
                }
            },
            new StepTypeMetadata
            {
                Type = "RunPowerShell",
                DisplayName = "Run PowerShell",
                Description = "Execute a PowerShell script",
                Icon = "code",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition { Name = "ScriptPath", Type = "string", Required = false, Description = "Path to PowerShell script" },
                    new PropertyDefinition { Name = "ScriptContent", Type = "string", Required = false, Description = "Inline PowerShell script" },
                    new PropertyDefinition { Name = "WorkingDirectory", Type = "string", Required = false, Description = "Working directory" },
                    new PropertyDefinition { Name = "Timeout", Type = "int", Required = false, DefaultValue = "3600", Description = "Timeout in seconds" }
                }
            },
            new StepTypeMetadata
            {
                Type = "SetVariable",
                DisplayName = "Set Variable",
                Description = "Set a task sequence variable",
                Icon = "variable",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition { Name = "VariableName", Type = "string", Required = true, Description = "Variable name" },
                    new PropertyDefinition { Name = "VariableValue", Type = "string", Required = true, Description = "Variable value" }
                }
            },
            new StepTypeMetadata
            {
                Type = "RestartComputer",
                DisplayName = "Restart Computer",
                Description = "Restart the computer",
                Icon = "refresh",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition { Name = "Timeout", Type = "int", Required = false, DefaultValue = "60", Description = "Timeout before restart in seconds" },
                    new PropertyDefinition { Name = "Message", Type = "string", Required = false, Description = "Message to display" }
                }
            },
            new StepTypeMetadata
            {
                Type = "FormatAndPartition",
                DisplayName = "Format and Partition",
                Description = "Format and partition disks",
                Icon = "hard-drive",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition { Name = "DiskNumber", Type = "int", Required = true, DefaultValue = "0", Description = "Disk number to partition" },
                    new PropertyDefinition { Name = "DiskType", Type = "string", Required = false, DefaultValue = "GPT", Description = "Partition style (GPT or MBR)" },
                    new PropertyDefinition { Name = "SystemPartitionSize", Type = "int", Required = false, DefaultValue = "300", Description = "System partition size in MB" }
                }
            },
            new StepTypeMetadata
            {
                Type = "Custom",
                DisplayName = "Custom Step",
                Description = "Custom step with user-defined properties",
                Icon = "settings",
                Properties = new List<PropertyDefinition>()
            }
        };
    }

    public StepTypeMetadata? GetStepTypeMetadata(string stepType)
    {
        return GetAllStepTypes().FirstOrDefault(s => s.Type.Equals(stepType, StringComparison.OrdinalIgnoreCase));
    }
}
