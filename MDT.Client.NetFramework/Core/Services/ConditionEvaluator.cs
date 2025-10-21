using System;
using System.Collections.Generic;
using MDT.Client.NetFramework.Core.Models;

namespace MDT.Client.NetFramework.Core.Services
{
    /// <summary>
    /// Evaluates task sequence conditions
    /// </summary>
    public class ConditionEvaluator
    {
        private readonly VariableManager _variableManager;

        public ConditionEvaluator(VariableManager variableManager)
        {
            _variableManager = variableManager ?? throw new ArgumentNullException("variableManager");
        }

        /// <summary>
        /// Evaluates all conditions for a step (AND logic)
        /// </summary>
        public bool EvaluateConditions(List<TaskSequenceCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0)
                return true;

            foreach (TaskSequenceCondition condition in conditions)
            {
                if (!EvaluateCondition(condition))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Evaluates a single condition
        /// </summary>
        public bool EvaluateCondition(TaskSequenceCondition condition)
        {
            if (condition == null)
                return true;

            string type = condition.Type ?? string.Empty;

            switch (type.ToUpperInvariant())
            {
                case "VARIABLE":
                    return EvaluateVariableCondition(condition);

                case "WMI":
                    return EvaluateWmiCondition(condition);

                case "REGISTRY":
                    return EvaluateRegistryCondition(condition);

                case "FILE":
                    return EvaluateFileCondition(condition);

                case "FOLDER":
                    return EvaluateFolderCondition(condition);

                case "SOFTWARE":
                    return EvaluateSoftwareCondition(condition);

                default:
                    // Unknown condition type - default to true to continue execution
                    return true;
            }
        }

        private bool EvaluateVariableCondition(TaskSequenceCondition condition)
        {
            string varName = GetProperty(condition, "Variable");
            string varValue = GetProperty(condition, "Value");
            string operatorType = GetProperty(condition, "Operator");

            string actualValue = _variableManager.GetVariable(varName);

            switch (operatorType.ToUpperInvariant())
            {
                case "EQUALS":
                    return string.Equals(actualValue, varValue, StringComparison.OrdinalIgnoreCase);

                case "NOTEQUALS":
                    return !string.Equals(actualValue, varValue, StringComparison.OrdinalIgnoreCase);

                case "GREATER":
                    return CompareNumeric(actualValue, varValue) > 0;

                case "GREATEROREQUAL":
                    return CompareNumeric(actualValue, varValue) >= 0;

                case "LESS":
                    return CompareNumeric(actualValue, varValue) < 0;

                case "LESSOREQUAL":
                    return CompareNumeric(actualValue, varValue) <= 0;

                case "EXISTS":
                    return _variableManager.VariableExists(varName);

                case "NOTEXISTS":
                    return !_variableManager.VariableExists(varName);

                default:
                    return string.Equals(actualValue, varValue, StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool EvaluateWmiCondition(TaskSequenceCondition condition)
        {
            // WMI query evaluation - simplified for now
            // In full implementation, would use System.Management
            return true;
        }

        private bool EvaluateRegistryCondition(TaskSequenceCondition condition)
        {
            // Registry check - simplified for now
            // In full implementation, would use Microsoft.Win32.Registry
            return true;
        }

        private bool EvaluateFileCondition(TaskSequenceCondition condition)
        {
            string path = GetProperty(condition, "Path");
            if (string.IsNullOrEmpty(path))
                return false;

            path = _variableManager.ExpandVariables(path);
            return System.IO.File.Exists(path);
        }

        private bool EvaluateFolderCondition(TaskSequenceCondition condition)
        {
            string path = GetProperty(condition, "Path");
            if (string.IsNullOrEmpty(path))
                return false;

            path = _variableManager.ExpandVariables(path);
            return System.IO.Directory.Exists(path);
        }

        private bool EvaluateSoftwareCondition(TaskSequenceCondition condition)
        {
            // Software detection - simplified for now
            // In full implementation, would check registry for installed software
            return true;
        }

        private string GetProperty(TaskSequenceCondition condition, string key)
        {
            string value;
            if (condition.Properties.TryGetValue(key, out value))
                return value ?? string.Empty;

            return string.Empty;
        }

        private int CompareNumeric(string value1, string value2)
        {
            double num1, num2;

            if (double.TryParse(value1, out num1) && double.TryParse(value2, out num2))
            {
                return num1.CompareTo(num2);
            }

            // Fall back to string comparison
            return string.Compare(value1, value2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
