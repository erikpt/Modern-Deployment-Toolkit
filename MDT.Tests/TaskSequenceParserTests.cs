using MDT.TaskSequence.Parsers;
using Xunit;

namespace MDT.Tests;

public class TaskSequenceParserTests
{
    [Fact]
    public void XmlParser_CanParse_ValidXml_ShouldReturnTrue()
    {
        var parser = new XmlTaskSequenceParser();
        var xml = @"<?xml version=""1.0""?>
<TaskSequence id=""test"" name=""Test"">
    <steps></steps>
</TaskSequence>";

        var result = parser.CanParse(xml);
        Assert.True(result);
    }

    [Fact]
    public void XmlParser_Parse_ShouldCreateTaskSequence()
    {
        var parser = new XmlTaskSequenceParser();
        var xml = @"<?xml version=""1.0""?>
<TaskSequence id=""test-001"" name=""Test Sequence"" version=""1.0.0"">
    <description>Test Description</description>
    <variables>
        <variable name=""Var1"" value=""Value1"" readonly=""false"" secret=""false"" />
    </variables>
    <steps>
        <step id=""step-001"" name=""Test Step"" type=""SetVariable"" enabled=""true"" continueOnError=""false"">
            <properties>
                <VariableName>TestVar</VariableName>
                <VariableValue>TestValue</VariableValue>
            </properties>
        </step>
    </steps>
</TaskSequence>";

        var taskSequence = parser.Parse(xml);
        
        Assert.Equal("test-001", taskSequence.Id);
        Assert.Equal("Test Sequence", taskSequence.Name);
        Assert.Equal("Test Description", taskSequence.Description);
        Assert.Single(taskSequence.Variables);
        Assert.Single(taskSequence.Steps);
    }

    [Fact]
    public void JsonParser_CanParse_ValidJson_ShouldReturnTrue()
    {
        var parser = new JsonTaskSequenceParser();
        var json = @"{""name"": ""Test"", ""steps"": []}";

        var result = parser.CanParse(json);
        Assert.True(result);
    }

    [Fact]
    public void YamlParser_CanParse_ValidYaml_ShouldReturnTrue()
    {
        var parser = new YamlTaskSequenceParser();
        var yaml = @"name: Test
steps: []";

        var result = parser.CanParse(yaml);
        Assert.True(result);
    }
}
