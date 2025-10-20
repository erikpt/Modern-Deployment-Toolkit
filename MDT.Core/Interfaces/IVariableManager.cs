using MDT.Core.Models;

namespace MDT.Core.Interfaces;

public interface IVariableManager
{
    string GetVariable(string name);
    void SetVariable(string name, string value);
    bool VariableExists(string name);
    Dictionary<string, string> GetAllVariables();
    void ClearVariables();
    string ExpandVariables(string input);
}
