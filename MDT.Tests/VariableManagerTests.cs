using MDT.Core.Services;
using Xunit;

namespace MDT.Tests;

public class VariableManagerTests
{
    [Fact]
    public void SetVariable_ShouldStoreVariable()
    {
        var manager = new VariableManager();
        manager.SetVariable("TestVar", "TestValue");
        
        var value = manager.GetVariable("TestVar");
        Assert.Equal("TestValue", value);
    }

    [Fact]
    public void GetVariable_WhenNotExists_ShouldReturnEmpty()
    {
        var manager = new VariableManager();
        var value = manager.GetVariable("NonExistent");
        
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void VariableExists_WhenExists_ShouldReturnTrue()
    {
        var manager = new VariableManager();
        manager.SetVariable("TestVar", "TestValue");
        
        Assert.True(manager.VariableExists("TestVar"));
    }

    [Fact]
    public void VariableExists_WhenNotExists_ShouldReturnFalse()
    {
        var manager = new VariableManager();
        Assert.False(manager.VariableExists("NonExistent"));
    }

    [Fact]
    public void ExpandVariables_ShouldReplaceVariables()
    {
        var manager = new VariableManager();
        manager.SetVariable("ComputerName", "PC001");
        manager.SetVariable("Domain", "contoso.com");
        
        var expanded = manager.ExpandVariables("Computer: %ComputerName%.%Domain%");
        Assert.Equal("Computer: PC001.contoso.com", expanded);
    }

    [Fact]
    public void ClearVariables_ShouldRemoveAllVariables()
    {
        var manager = new VariableManager();
        manager.SetVariable("Var1", "Value1");
        manager.SetVariable("Var2", "Value2");
        
        manager.ClearVariables();
        
        Assert.False(manager.VariableExists("Var1"));
        Assert.False(manager.VariableExists("Var2"));
    }
}
