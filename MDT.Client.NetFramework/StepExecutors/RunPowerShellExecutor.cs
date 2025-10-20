using System;
using MDT.Client.NetFramework.Core.Models;
using MDT.Client.NetFramework.Core.Services;

namespace MDT.Client.NetFramework.StepExecutors
{
    public class RunPowerShellExecutor : BaseStepExecutor
    {
        public RunPowerShellExecutor(VariableManager variableManager) : base(variableManager) { }
        public override string SupportedStepType { get { return "BDD_RunPowerShellAction"; } }
        public override StepExecutionResult Execute(TaskSequenceStep step, ExecutionContext context)
        {
            // TODO: Implement PowerShell execution
            Log("Running PowerShell script - not yet implemented");
            return CreateSuccessResult(step);
        }
    }
}
