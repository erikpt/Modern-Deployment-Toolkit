using MDT.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDT.Plugins.Steps;

public class FormatAndPartitionExecutor : BaseStepExecutor
{
    public FormatAndPartitionExecutor(ILogger<FormatAndPartitionExecutor> logger) : base(logger)
    {
    }

    public override StepType SupportedStepType => StepType.FormatAndPartition;

    public override async Task<StepExecutionResult> ExecuteAsync(
        TaskSequenceStep step,
        MDT.Core.Models.ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var result = new StepExecutionResult
        {
            StepId = step.Id,
            StepName = step.Name,
            Status = ExecutionStatus.Running,
            StartTime = DateTime.UtcNow
        };

        try
        {
            Logger.LogInformation("Formatting and partitioning disk");

            var diskNumber = step.Properties.GetValueOrDefault("DiskNumber", "0");
            var partitionStyle = step.Properties.GetValueOrDefault("PartitionStyle", "GPT");

            Logger.LogInformation("Preparing disk {DiskNumber} with {PartitionStyle} partition style", diskNumber, partitionStyle);

            await Task.Delay(100, cancellationToken);

            result.Status = ExecutionStatus.Completed;
            result.OutputVariables["DiskPrepared"] = "true";
            result.OutputVariables["DiskNumber"] = diskNumber;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to format and partition disk");
            result.Status = ExecutionStatus.Failed;
            result.ErrorMessage = ex.Message;
        }
        finally
        {
            result.EndTime = DateTime.UtcNow;
        }

        return result;
    }
}
