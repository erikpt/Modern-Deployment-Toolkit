using System;
using MDT.Client.NetFramework.Core.Models;
using MDT.Client.NetFramework.Core.Services;

namespace MDT.Client.NetFramework.StepExecutors
{
    /// <summary>
    /// Executes Check BIOS step
    /// Verifies BIOS/UEFI settings
    /// </summary>
    public class CheckBiosExecutor : BaseStepExecutor
    {
        public CheckBiosExecutor(VariableManager variableManager)
            : base(variableManager)
        {
        }

        public override string SupportedStepType
        {
            get { return "Check BIOS"; }
        }

        public override bool CanExecute(TaskSequenceStep step)
        {
            return string.Equals(step.Type, SupportedStepType, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(step.Name, "Check BIOS", StringComparison.OrdinalIgnoreCase);
        }

        public override StepExecutionResult Execute(TaskSequenceStep step, ExecutionContext context)
        {
            StepExecutionResult result = new StepExecutionResult
            {
                StepId = step.Id,
                StepName = step.Name,
                StartTime = DateTime.UtcNow,
                Status = ExecutionStatus.Running
            };

            try
            {
                Log("Checking BIOS/UEFI settings");

                // TODO: Implement BIOS check via WMI
                // - Check if UEFI or Legacy BIOS
                // - Verify Secure Boot status
                // - Check TPM version
                // - Validate virtualization settings

                result.Status = ExecutionStatus.Completed;
                result.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Log("BIOS check error: " + ex.Message);
                result.Status = ExecutionStatus.Failed;
                result.ErrorMessage = ex.Message;
                result.ExitCode = 1;
            }
            finally
            {
                result.EndTime = DateTime.UtcNow;
            }

            return result;
        }
    }
}
