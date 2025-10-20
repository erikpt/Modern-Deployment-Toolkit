using MDT.Core.Interfaces;
using MDT.Core.Models;
using System.Xml.Linq;

namespace MDT.TaskSequence.Parsers;

public class XmlTaskSequenceParser : ITaskSequenceParser
{
    public bool CanParse(string content)
    {
        try
        {
            var doc = XDocument.Parse(content);
            return doc.Root?.Name.LocalName == "sequence" || doc.Root?.Name.LocalName == "TaskSequence";
        }
        catch
        {
            return false;
        }
    }

    public Core.Models.TaskSequence Parse(string content)
    {
        var doc = XDocument.Parse(content);
        var root = doc.Root ?? throw new InvalidOperationException("Invalid XML: no root element");

        var taskSequence = new Core.Models.TaskSequence
        {
            Id = root.Attribute("id")?.Value ?? Guid.NewGuid().ToString(),
            Name = root.Attribute("name")?.Value ?? "Unnamed Task Sequence",
            Description = root.Element("description")?.Value ?? string.Empty,
            Version = root.Attribute("version")?.Value ?? "1.0.0"
        };

        var variablesElement = root.Element("variables");
        if (variablesElement != null)
        {
            foreach (var varElement in variablesElement.Elements("variable"))
            {
                taskSequence.Variables.Add(new TaskSequenceVariable
                {
                    Name = varElement.Attribute("name")?.Value ?? string.Empty,
                    Value = varElement.Attribute("value")?.Value ?? string.Empty,
                    IsReadOnly = bool.Parse(varElement.Attribute("readonly")?.Value ?? "false"),
                    IsSecret = bool.Parse(varElement.Attribute("secret")?.Value ?? "false")
                });
            }
        }

        var stepsElement = root.Element("steps");
        if (stepsElement != null)
        {
            foreach (var stepElement in stepsElement.Elements())
            {
                taskSequence.Steps.Add(ParseStep(stepElement));
            }
        }

        return taskSequence;
    }

    private TaskSequenceStep ParseStep(XElement element)
    {
        var step = new TaskSequenceStep
        {
            Id = element.Attribute("id")?.Value ?? Guid.NewGuid().ToString(),
            Name = element.Attribute("name")?.Value ?? string.Empty,
            Description = element.Element("description")?.Value ?? string.Empty,
            Type = ParseStepType(element.Attribute("type")?.Value ?? element.Name.LocalName),
            Enabled = bool.Parse(element.Attribute("enabled")?.Value ?? "true"),
            ContinueOnError = bool.Parse(element.Attribute("continueOnError")?.Value ?? "false")
        };

        var propertiesElement = element.Element("properties");
        if (propertiesElement != null)
        {
            foreach (var prop in propertiesElement.Elements())
            {
                step.Properties[prop.Name.LocalName] = prop.Value;
            }
        }

        var conditionsElement = element.Element("conditions");
        if (conditionsElement != null)
        {
            foreach (var condElement in conditionsElement.Elements("condition"))
            {
                step.Conditions.Add(new TaskSequenceCondition
                {
                    VariableName = condElement.Attribute("variable")?.Value ?? string.Empty,
                    Operator = ParseOperator(condElement.Attribute("operator")?.Value ?? "Equals"),
                    Value = condElement.Attribute("value")?.Value ?? string.Empty
                });
            }
        }

        var childStepsElement = element.Element("steps");
        if (childStepsElement != null)
        {
            foreach (var childElement in childStepsElement.Elements())
            {
                step.ChildSteps.Add(ParseStep(childElement));
            }
        }

        return step;
    }

    private StepType ParseStepType(string typeString)
    {
        return typeString.ToLowerInvariant() switch
        {
            "group" => StepType.Group,
            "installos" or "installoperatingsystem" => StepType.InstallOperatingSystem,
            "applyimage" or "applywindowsimage" => StepType.ApplyWindowsImage,
            "applyffu" or "applyffuimage" => StepType.ApplyFFUImage,
            "installapplication" => StepType.InstallApplication,
            "installdriver" => StepType.InstallDriver,
            "captureuserstate" => StepType.CaptureUserState,
            "restoreuserstate" => StepType.RestoreUserState,
            "runcommandline" => StepType.RunCommandLine,
            "runpowershell" => StepType.RunPowerShell,
            "setvariable" => StepType.SetVariable,
            "restart" or "restartcomputer" => StepType.RestartComputer,
            "formatandpartition" => StepType.FormatAndPartition,
            _ => StepType.Custom
        };
    }

    private ConditionOperator ParseOperator(string operatorString)
    {
        return operatorString.ToLowerInvariant() switch
        {
            "equals" or "=" => ConditionOperator.Equals,
            "notequals" or "!=" => ConditionOperator.NotEquals,
            "greaterthan" or ">" => ConditionOperator.GreaterThan,
            "lessthan" or "<" => ConditionOperator.LessThan,
            "contains" => ConditionOperator.Contains,
            "exists" => ConditionOperator.Exists,
            _ => ConditionOperator.Equals
        };
    }

    public string Serialize(Core.Models.TaskSequence taskSequence)
    {
        var root = new XElement("TaskSequence",
            new XAttribute("id", taskSequence.Id),
            new XAttribute("name", taskSequence.Name),
            new XAttribute("version", taskSequence.Version)
        );

        if (!string.IsNullOrEmpty(taskSequence.Description))
        {
            root.Add(new XElement("description", taskSequence.Description));
        }

        if (taskSequence.Variables.Count > 0)
        {
            var variablesElement = new XElement("variables");
            foreach (var variable in taskSequence.Variables)
            {
                variablesElement.Add(new XElement("variable",
                    new XAttribute("name", variable.Name),
                    new XAttribute("value", variable.Value),
                    new XAttribute("readonly", variable.IsReadOnly),
                    new XAttribute("secret", variable.IsSecret)
                ));
            }
            root.Add(variablesElement);
        }

        if (taskSequence.Steps.Count > 0)
        {
            var stepsElement = new XElement("steps");
            foreach (var step in taskSequence.Steps)
            {
                stepsElement.Add(SerializeStep(step));
            }
            root.Add(stepsElement);
        }

        return new XDocument(root).ToString();
    }

    private XElement SerializeStep(TaskSequenceStep step)
    {
        var element = new XElement("step",
            new XAttribute("id", step.Id),
            new XAttribute("name", step.Name),
            new XAttribute("type", step.Type.ToString()),
            new XAttribute("enabled", step.Enabled),
            new XAttribute("continueOnError", step.ContinueOnError)
        );

        if (!string.IsNullOrEmpty(step.Description))
        {
            element.Add(new XElement("description", step.Description));
        }

        if (step.Properties.Count > 0)
        {
            var propertiesElement = new XElement("properties");
            foreach (var prop in step.Properties)
            {
                propertiesElement.Add(new XElement(prop.Key, prop.Value));
            }
            element.Add(propertiesElement);
        }

        if (step.Conditions.Count > 0)
        {
            var conditionsElement = new XElement("conditions");
            foreach (var condition in step.Conditions)
            {
                conditionsElement.Add(new XElement("condition",
                    new XAttribute("variable", condition.VariableName),
                    new XAttribute("operator", condition.Operator.ToString()),
                    new XAttribute("value", condition.Value)
                ));
            }
            element.Add(conditionsElement);
        }

        if (step.ChildSteps.Count > 0)
        {
            var childStepsElement = new XElement("steps");
            foreach (var childStep in step.ChildSteps)
            {
                childStepsElement.Add(SerializeStep(childStep));
            }
            element.Add(childStepsElement);
        }

        return element;
    }
}
