using System;
using MDT.Client.NetFramework.Core.Models;
using MDT.Client.NetFramework.Core.Services;

namespace MDT.Client.NetFramework.StepExecutors
{
    public class BddInstallOsExecutor : BaseStepExecutor
    {
        public BddInstallOsExecutor(VariableManager variableManager) : base(variableManager) { }
        public override string SupportedStepType { get { return "BDD_InstallOS"; } }
        public override StepExecutionResult Execute(TaskSequenceStep step, ExecutionContext context)
        {
            // TODO: Implement OS installation via DISM
            Log("Installing OS - not yet implemented");
            return CreateSuccessResult(step);
        }
    }
}
