using MDT.Core.Interfaces;
using System.Text.RegularExpressions;

namespace MDT.Core.Services;

public class VariableManager : IVariableManager
{
    private readonly Dictionary<string, string> _variables = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _readOnlyVariables = new(StringComparer.OrdinalIgnoreCase);

    public string GetVariable(string name)
    {
        return _variables.TryGetValue(name, out var value) ? value : string.Empty;
    }

    public void SetVariable(string name, string value)
    {
        if (_readOnlyVariables.Contains(name))
        {
            throw new InvalidOperationException($"Variable '{name}' is read-only and cannot be modified.");
        }
        _variables[name] = value;
    }

    public bool VariableExists(string name)
    {
        return _variables.ContainsKey(name);
    }

    public Dictionary<string, string> GetAllVariables()
    {
        return new Dictionary<string, string>(_variables, StringComparer.OrdinalIgnoreCase);
    }

    public void ClearVariables()
    {
        _variables.Clear();
        _readOnlyVariables.Clear();
    }

    public string ExpandVariables(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return Regex.Replace(input, @"%([^%]+)%", match =>
        {
            var variableName = match.Groups[1].Value;
            return GetVariable(variableName);
        });
    }

    public void SetReadOnlyVariable(string name, string value)
    {
        _variables[name] = value;
        _readOnlyVariables.Add(name);
    }
}
