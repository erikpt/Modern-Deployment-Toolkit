namespace MDT.Core.Models;

public enum StepType
{
    Group,
    InstallOperatingSystem,
    ApplyWindowsImage,
    ApplyFFUImage,
    InstallApplication,
    InstallDriver,
    CaptureUserState,
    RestoreUserState,
    RunCommandLine,
    RunPowerShell,
    SetVariable,
    RestartComputer,
    FormatAndPartition,
    Custom
}

public class TaskSequenceStep
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StepType Type { get; set; }
    public bool Enabled { get; set; } = true;
    public bool ContinueOnError { get; set; }
    public List<TaskSequenceCondition> Conditions { get; set; } = new();
    public Dictionary<string, string> Properties { get; set; } = new();
    public List<TaskSequenceStep> ChildSteps { get; set; } = new();
}
