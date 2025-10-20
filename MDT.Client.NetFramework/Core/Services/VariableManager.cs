using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MDT.Client.NetFramework.Core.Services
{
    /// <summary>
    /// Manages task sequence variables with MDT compatibility
    /// </summary>
    public class VariableManager
    {
        private readonly Dictionary<string, string> _variables;
        private readonly HashSet<string> _readOnlyVariables;

        public VariableManager()
        {
            _variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _readOnlyVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets a variable value
        /// </summary>
        public string GetVariable(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            string value;
            if (_variables.TryGetValue(name, out value))
                return value;

            return string.Empty;
        }

        /// <summary>
        /// Sets a variable value
        /// </summary>
        public void SetVariable(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Variable name cannot be null or empty");

            if (_readOnlyVariables.Contains(name))
                throw new InvalidOperationException(string.Format("Variable '{0}' is read-only", name));

            _variables[name] = value ?? string.Empty;
        }

        /// <summary>
        /// Sets a read-only variable
        /// </summary>
        public void SetReadOnlyVariable(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Variable name cannot be null or empty");

            _variables[name] = value ?? string.Empty;
            _readOnlyVariables.Add(name);
        }

        /// <summary>
        /// Checks if a variable exists
        /// </summary>
        public bool VariableExists(string name)
        {
            return !string.IsNullOrEmpty(name) && _variables.ContainsKey(name);
        }

        /// <summary>
        /// Expands variables in a string using %VariableName% syntax
        /// </summary>
        public string ExpandVariables(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Use regex to find and replace %variable% patterns
            return Regex.Replace(input, @"%([^%]+)%", 
                delegate(Match match)
                {
                    string varName = match.Groups[1].Value;
                    return GetVariable(varName);
                });
        }

        /// <summary>
        /// Gets all variables as a dictionary
        /// </summary>
        public Dictionary<string, string> GetAllVariables()
        {
            return new Dictionary<string, string>(_variables, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Clears all variables
        /// </summary>
        public void ClearVariables()
        {
            _variables.Clear();
            _readOnlyVariables.Clear();
        }
    }
}
