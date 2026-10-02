using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace WordBattle.Tests
{
    /// <summary>
    /// Writes a summary of every test run (EditMode or PlayMode, from any trigger) to
    /// Temp/TestResults.txt, so tooling such as the Unity MCP can read results after
    /// PlayMode's domain reloads.
    /// </summary>
    [InitializeOnLoad]
    internal static class TestResultsWriter
    {
        private static readonly string OutputPath = Path.Combine(Application.dataPath, "../Temp/TestResults.txt");

        static TestResultsWriter()
        {
            ScriptableObject.CreateInstance<TestRunnerApi>().RegisterCallbacks(new Callbacks());
        }

        private sealed class Callbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                var summary = new StringBuilder();
                summary.AppendLine($"run={result.Name} passed={result.PassCount} failed={result.FailCount} " +
                                   $"skipped={result.SkipCount} inconclusive={result.InconclusiveCount}");
                AppendFailures(result, summary);
                File.WriteAllText(OutputPath, summary.ToString());
            }

            private static void AppendFailures(ITestResultAdaptor result, StringBuilder summary)
            {
                if (!result.HasChildren)
                {
                    if (result.TestStatus == TestStatus.Failed)
                    {
                        summary.AppendLine($"FAILED {result.FullName}: {result.Message}");
                    }
                    return;
                }

                foreach (ITestResultAdaptor child in result.Children)
                {
                    AppendFailures(child, summary);
                }
            }
        }
    }
}
