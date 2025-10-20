using MDT.Core.Interfaces;
using MDT.Core.Services;
using MDT.TaskSequence.Executors;
using MDT.TaskSequence.Parsers;
using MDT.Plugins.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("==============================================");
Console.WriteLine("Modern Deployment Toolkit - Task Sequence Engine");
Console.WriteLine("==============================================");
Console.WriteLine();

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

services.AddSingleton<IVariableManager, VariableManager>();
services.AddSingleton<IConditionEvaluator, ConditionEvaluator>();

services.AddTransient<ITaskSequenceParser, XmlTaskSequenceParser>();
services.AddTransient<ITaskSequenceParser, JsonTaskSequenceParser>();
services.AddTransient<ITaskSequenceParser, YamlTaskSequenceParser>();

services.AddTransient<IStepExecutor, ApplyWindowsImageExecutor>();
services.AddTransient<IStepExecutor, ApplyFFUImageExecutor>();
services.AddTransient<IStepExecutor, InstallApplicationExecutor>();
services.AddTransient<IStepExecutor, InstallDriverExecutor>();
services.AddTransient<IStepExecutor, CaptureUserStateExecutor>();
services.AddTransient<IStepExecutor, RestoreUserStateExecutor>();
services.AddTransient<IStepExecutor, RunCommandLineExecutor>();
services.AddTransient<IStepExecutor, RunPowerShellExecutor>();
services.AddTransient<IStepExecutor, SetVariableExecutor>();
services.AddTransient<IStepExecutor, FormatAndPartitionExecutor>();
services.AddTransient<IStepExecutor, RestartComputerExecutor>();

services.AddTransient<TaskSequenceEngine>();

var serviceProvider = services.BuildServiceProvider();

if (args.Length == 0)
{
    Console.WriteLine("Usage: MDT.Engine <task-sequence-file>");
    Console.WriteLine();
    Console.WriteLine("Supported formats: XML, JSON, YAML");
    return 1;
}

var filePath = args[0];

if (!File.Exists(filePath))
{
    Console.WriteLine($"Error: File not found: {filePath}");
    return 1;
}

try
{
    var content = await File.ReadAllTextAsync(filePath);
    var parsers = serviceProvider.GetServices<ITaskSequenceParser>();
    
    var parser = parsers.FirstOrDefault(p => p.CanParse(content));
    if (parser == null)
    {
        Console.WriteLine("Error: Unable to determine task sequence format");
        return 1;
    }

    Console.WriteLine($"Parsing task sequence from: {filePath}");
    var taskSequence = parser.Parse(content);
    Console.WriteLine($"Task Sequence: {taskSequence.Name}");
    Console.WriteLine($"Description: {taskSequence.Description}");
    Console.WriteLine($"Steps: {taskSequence.Steps.Count}");
    Console.WriteLine();

    var engine = serviceProvider.GetRequiredService<TaskSequenceEngine>();
    
    Console.WriteLine("Starting execution...");
    Console.WriteLine();

    var result = await engine.ExecuteAsync(taskSequence);

    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("Execution Summary");
    Console.WriteLine("==============================================");
    Console.WriteLine($"Status: {result.Status}");
    Console.WriteLine($"Started: {result.StartTime:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"Ended: {result.EndTime:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"Duration: {result.EndTime - result.StartTime}");
    Console.WriteLine($"Total Steps: {result.StepResults.Count}");
    Console.WriteLine($"Completed: {result.StepResults.Count(r => r.Status == MDT.Core.Models.ExecutionStatus.Completed)}");
    Console.WriteLine($"Failed: {result.StepResults.Count(r => r.Status == MDT.Core.Models.ExecutionStatus.Failed)}");
    Console.WriteLine();

    if (result.StepResults.Any(r => r.Status == MDT.Core.Models.ExecutionStatus.Failed))
    {
        Console.WriteLine("Failed Steps:");
        foreach (var failedStep in result.StepResults.Where(r => r.Status == MDT.Core.Models.ExecutionStatus.Failed))
        {
            Console.WriteLine($"  - {failedStep.StepName}: {failedStep.ErrorMessage}");
        }
    }

    return result.Status == MDT.Core.Models.ExecutionStatus.Completed ? 0 : 1;
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}
