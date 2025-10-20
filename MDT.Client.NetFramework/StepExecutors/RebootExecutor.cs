using System;
using MDT.Client.NetFramework.Core.Models;
using MDT.Client.NetFramework.Core.Services;

namespace MDT.Client.NetFramework.StepExecutors
{
    public class RebootExecutor : BaseStepExecutor
    {
        public RebootExecutor(VariableManager variableManager) : base(variableManager) { }
        public override string SupportedStepType { get { return "SMS_TaskSequence_RebootAction"; } }
        public override StepExecutionResult Execute(TaskSequenceStep step, ExecutionContext context)
        {
            // TODO: Implement reboot with task sequence resume
            Log("Rebooting computer - not yet implemented");
            return CreateSuccessResult(step);
        }
    }
}
