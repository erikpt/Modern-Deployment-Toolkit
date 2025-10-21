using System;
using System.IO;
using MDT.Client.NetFramework.Core.Models;
using MDT.Client.NetFramework.Parsers;

namespace MDT.Client.NetFramework
{
    /// <summary>
    /// Test program to validate XML parser with real MDT files
    /// </summary>
    public class TestParser
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("MDT XML Parser Test");
            Console.WriteLine("========================================");
            Console.WriteLine();

            string[] testFiles = new string[]
            {
                "../DeployXMLExamples/001/ts.xml",
                "../DeployXMLExamples/002/ts.xml",
                "../DeployXMLExamples/003/ts.xml"
            };

            MdtXmlParser parser = new MdtXmlParser();
            int passed = 0;
            int failed = 0;

            foreach (string testFile in testFiles)
            {
                Console.WriteLine("Testing: {0}", testFile);
                Console.WriteLine(new string('-', 60));

                try
                {
                    if (!File.Exists(testFile))
                    {
                        Console.WriteLine("  ERROR: File not found");
                        failed++;
                        Console.WriteLine();
                        continue;
                    }

                    string xml = File.ReadAllText(testFile);
                    
                    if (!parser.CanParse(xml))
                    {
                        Console.WriteLine("  ERROR: Parser cannot parse this file");
                        failed++;
                        Console.WriteLine();
                        continue;
                    }

                    TaskSequence ts = parser.Parse(xml);

                    Console.WriteLine("  Name: {0}", ts.Name);
                    Console.WriteLine("  Version: {0}", ts.Version);
                    Console.WriteLine("  Variables: {0}", ts.Variables.Count);
                    Console.WriteLine("  Steps: {0}", ts.Steps.Count);

                    // Count step types
                    System.Collections.Generic.Dictionary<string, int> stepCounts = 
                        new System.Collections.Generic.Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                    CountStepTypes(ts.Steps, stepCounts);

                    Console.WriteLine();
                    Console.WriteLine("  Step Type Summary:");
                    foreach (var kvp in stepCounts)
                    {
                        Console.WriteLine("    {0}: {1}", kvp.Key, kvp.Value);
                    }

                    Console.WriteLine();
                    Console.WriteLine("  ✓ PASSED");
                    passed++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("  ✗ FAILED: {0}", ex.Message);
                    Console.WriteLine("  Stack: {0}", ex.StackTrace);
                    failed++;
                }

                Console.WriteLine();
            }

            Console.WriteLine("========================================");
            Console.WriteLine("Test Summary");
            Console.WriteLine("========================================");
            Console.WriteLine("Passed: {0}", passed);
            Console.WriteLine("Failed: {0}", failed);
            Console.WriteLine();

            Environment.Exit(failed > 0 ? 1 : 0);
        }

        private static void CountStepTypes(
            System.Collections.Generic.List<TaskSequenceStep> steps,
            System.Collections.Generic.Dictionary<string, int> counts)
        {
            foreach (TaskSequenceStep step in steps)
            {
                string type = step.Type ?? "group";
                
                if (!counts.ContainsKey(type))
                {
                    counts[type] = 0;
                }
                counts[type]++;

                if (step.ChildSteps != null && step.ChildSteps.Count > 0)
                {
                    CountStepTypes(step.ChildSteps, counts);
                }
            }
        }
    }
}
