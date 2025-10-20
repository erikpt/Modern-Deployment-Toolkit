using System;
using MDT.Client.NetFramework.Core.Models;
using MDT.Client.NetFramework.Core.Services;

namespace MDT.Client.NetFramework.StepExecutors
{
    public class ZtiWinReExecutor : BaseStepExecutor
    {
        public ZtiWinReExecutor(VariableManager variableManager) : base(variableManager) { }
        public override string SupportedStepType { get { return "ZTIWinRE.wsf"; } }
        public override bool CanExecute(TaskSequenceStep step)
        {
            return base.CanExecute(step) || (step.Name != null && step.Name.Contains("WinRE"));
        }
        public override StepExecutionResult Execute(TaskSequenceStep step, ExecutionContext context)
        {
            // TODO: Implement WinRE configuration
            Log("Configuring WinRE - not yet implemented");
            return CreateSuccessResult(step);
        }
    }
}
