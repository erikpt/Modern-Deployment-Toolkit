using System;
using MDT.Client.NetFramework.Core.Models;
using MDT.Client.NetFramework.Core.Services;

namespace MDT.Client.NetFramework.StepExecutors
{
    public class PartitionDiskExecutor : BaseStepExecutor
    {
        public PartitionDiskExecutor(VariableManager variableManager) : base(variableManager) { }
        public override string SupportedStepType { get { return "SMS_TaskSequence_PartitionDiskAction"; } }
        public override StepExecutionResult Execute(TaskSequenceStep step, ExecutionContext context)
        {
            // TODO: Implement disk partitioning via DiskPart
            Log("Partitioning disk - not yet implemented");
            return CreateSuccessResult(step);
        }
    }
}
