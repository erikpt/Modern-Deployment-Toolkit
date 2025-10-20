using System;
using System.Collections.Generic;
using System.Xml;
using MDT.Client.NetFramework.Core.Interfaces;
using MDT.Client.NetFramework.Core.Models;

namespace MDT.Client.NetFramework.Parsers
{
    /// <summary>
    /// Parses MDT Deployment Workbench task sequence XML files
    /// </summary>
    public class MdtXmlParser : ITaskSequenceParser
    {
        public bool CanParse(string content)
        {
            if (string.IsNullOrEmpty(content))
                return false;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);
                
                // Check for MDT-specific elements
                return doc.SelectSingleNode("//sequence") != null ||
                       doc.SelectSingleNode("//ts:sequence", GetNamespaceManager(doc)) != null;
            }
            catch
            {
                return false;
            }
        }

        public TaskSequence Parse(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Content cannot be null or empty");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);

            XmlNamespaceManager nsmgr = GetNamespaceManager(doc);

            TaskSequence taskSequence = new TaskSequence();

            // Parse root sequence element
            XmlNode sequenceNode = doc.SelectSingleNode("//sequence") ??
                                  doc.SelectSingleNode("//ts:sequence", nsmgr);

            if (sequenceNode == null)
                throw new InvalidOperationException("No sequence element found in XML");

            // Parse attributes
            taskSequence.Id = GetAttributeValue(sequenceNode, "id") ?? Guid.NewGuid().ToString();
            taskSequence.Name = GetAttributeValue(sequenceNode, "name") ?? "Unnamed Sequence";
            taskSequence.Version = GetAttributeValue(sequenceNode, "version") ?? "1.0";
            taskSequence.Description = GetAttributeValue(sequenceNode, "description") ?? string.Empty;

            // Parse variables
            XmlNode globalVarsNode = sequenceNode.SelectSingleNode("globalVarList");
            if (globalVarsNode != null)
            {
                ParseVariables(globalVarsNode, taskSequence.Variables);
            }

            // Parse steps/groups
            XmlNode stepsNode = sequenceNode.SelectSingleNode("steps") ?? sequenceNode.SelectSingleNode("group");
            if (stepsNode != null)
            {
                ParseSteps(stepsNode, taskSequence.Steps);
            }

            return taskSequence;
        }

        private void ParseVariables(XmlNode varsNode, List<TaskSequenceVariable> variables)
        {
            if (varsNode == null)
                return;

            foreach (XmlNode varNode in varsNode.SelectNodes("variable"))
            {
                TaskSequenceVariable variable = new TaskSequenceVariable
                {
                    Name = GetAttributeValue(varNode, "name") ?? GetAttributeValue(varNode, "property"),
                    Value = varNode.InnerText ?? GetAttributeValue(varNode, "value") ?? string.Empty
                };

                variables.Add(variable);
            }
        }

        private void ParseSteps(XmlNode stepsNode, List<TaskSequenceStep> steps)
        {
            if (stepsNode == null)
                return;

            foreach (XmlNode stepNode in stepsNode.ChildNodes)
            {
                if (stepNode.NodeType != XmlNodeType.Element)
                    continue;

                TaskSequenceStep step = ParseStep(stepNode);
                if (step != null)
                    steps.Add(step);
            }
        }

        private TaskSequenceStep ParseStep(XmlNode stepNode)
        {
            if (stepNode == null)
                return null;

            TaskSequenceStep step = new TaskSequenceStep();

            // Parse basic attributes
            step.Id = GetAttributeValue(stepNode, "id") ?? Guid.NewGuid().ToString();
            step.Name = GetAttributeValue(stepNode, "name") ?? GetAttributeValue(stepNode, "description") ?? "Unnamed Step";
            step.Description = GetAttributeValue(stepNode, "description") ?? string.Empty;
            step.Type = GetAttributeValue(stepNode, "type") ?? stepNode.Name;
            
            // Parse disable flag
            string disable = GetAttributeValue(stepNode, "disable");
            step.Enabled = !string.Equals(disable, "true", StringComparison.OrdinalIgnoreCase);

            // Parse continueOnError
            string continueOnError = GetAttributeValue(stepNode, "continueOnError");
            step.ContinueOnError = string.Equals(continueOnError, "true", StringComparison.OrdinalIgnoreCase);

            // Parse default variables (properties specific to this step)
            XmlNode defaultVarsNode = stepNode.SelectSingleNode("defaultVarList");
            if (defaultVarsNode != null)
            {
                foreach (XmlNode varNode in defaultVarsNode.SelectNodes("variable"))
                {
                    string name = GetAttributeValue(varNode, "name") ?? GetAttributeValue(varNode, "property");
                    string value = varNode.InnerText ?? GetAttributeValue(varNode, "value") ?? string.Empty;
                    
                    if (!string.IsNullOrEmpty(name))
                        step.Properties[name] = value;
                }
            }

            // Parse all attributes as properties
            if (stepNode.Attributes != null)
            {
                foreach (XmlAttribute attr in stepNode.Attributes)
                {
                    if (!step.Properties.ContainsKey(attr.Name))
                        step.Properties[attr.Name] = attr.Value;
                }
            }

            // Parse conditions
            XmlNode conditionNode = stepNode.SelectSingleNode("condition");
            if (conditionNode != null)
            {
                ParseCondition(conditionNode, step.Conditions);
            }

            // Parse child steps/groups
            XmlNode childStepsNode = stepNode.SelectSingleNode("steps") ?? stepNode.SelectSingleNode("subtasksequence");
            if (childStepsNode != null)
            {
                ParseSteps(childStepsNode, step.ChildSteps);
            }

            return step;
        }

        private void ParseCondition(XmlNode conditionNode, List<TaskSequenceCondition> conditions)
        {
            if (conditionNode == null)
                return;

            // Handle different condition types
            foreach (XmlNode node in conditionNode.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;

                TaskSequenceCondition condition = new TaskSequenceCondition
                {
                    Type = node.Name,
                    Expression = node.InnerText
                };

                // Parse condition attributes as properties
                if (node.Attributes != null)
                {
                    foreach (XmlAttribute attr in node.Attributes)
                    {
                        condition.Properties[attr.Name] = attr.Value;
                    }
                }

                // Parse child elements as properties
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        condition.Properties[child.Name] = child.InnerText;
                    }
                }

                conditions.Add(condition);
            }
        }

        private string GetAttributeValue(XmlNode node, string attributeName)
        {
            if (node == null || node.Attributes == null)
                return null;

            XmlAttribute attr = node.Attributes[attributeName];
            return attr != null ? attr.Value : null;
        }

        private XmlNamespaceManager GetNamespaceManager(XmlDocument doc)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ts", "http://schemas.microsoft.com/2003/08/TaskSequence");
            return nsmgr;
        }
    }
}
