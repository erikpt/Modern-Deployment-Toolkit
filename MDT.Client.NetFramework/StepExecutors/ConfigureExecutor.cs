using System;
using MDT.Client.NetFramework.Core.Models;
using MDT.Client.NetFramework.Core.Services;

namespace MDT.Client.NetFramework.StepExecutors
{
    public class ConfigureExecutor : BaseStepExecutor
    {
        public ConfigureExecutor(VariableManager variableManager) : base(variableManager) { }
        public override string SupportedStepType { get { return "Configure"; } }
        public override StepExecutionResult Execute(TaskSequenceStep step, ExecutionContext context)
        {
            // TODO: Implement configuration
            Log("Configuring deployment settings - not yet implemented");
            return CreateSuccessResult(step);
        }
    }
}
