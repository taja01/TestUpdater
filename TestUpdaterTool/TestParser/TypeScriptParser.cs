using Common;
using System.Text.RegularExpressions;

namespace TestParser
{
    public class TypeScriptParser : ITestCaseParser
    {

        private static readonly Regex TestBlockRegex = new(
           @"test\s*\(\s*\{(?:[^{}]*|\{(?:[^{}]*|\{.*?\})*\})*\}\s*,\s*\{(?:[^{}]*|\{(?:[^{}]*|\{.*?\})*\})*\}\s*,\s*async\s*\([^)]*\)\s*=>\s*\{(?:[^{}]*|\{(?:[^{}]*|\{.*?\})*\})*\}\s*\)",
            RegexOptions.Singleline
        );

        private static readonly Regex TestStepRegex = new(
            @"test\.step\(['\""](.+?)['\""], async \(\) => \{",
            RegexOptions.Singleline
        );

        public string FilePattern => "*.system.spec.ts";

        // Parse the content of a file
        public List<ParsedTest> ParseFile(string fileContent)
        {
            var parsedTests = new List<ParsedTest>();

            // Match all `test(...)` blocks
            var testBlockMatches = TestBlockRegex.Matches(fileContent);
            foreach (Match testBlockMatch in testBlockMatches)
            {
                var testBlock = testBlockMatch.Value;
                var parsedTest = ParseTestBlock(testBlock);
                if (parsedTest != null)
                {
                    parsedTests.Add(parsedTest);
                }
            }

            return parsedTests;
        }

        // Parse a single test block
        private static ParsedTest? ParseTestBlock(string testBlock)
        {
            var parsedTest = new ParsedTest
            {
                // Extract title
                Title = ExtractValue(testBlock, @"title:\s*'([^']*)'")
            };

            // Extract testCaseId
            var testCaseIdValue = ExtractValue(testBlock, @"testCaseId:\s*(\d+)");
            parsedTest.TestCaseId = int.TryParse(testCaseIdValue, out var testCaseId) ? testCaseId : null;

            // Extract all test steps
            var stepsMatches = TestStepRegex.Matches(testBlock);
            foreach (Match stepMatch in stepsMatches)
            {
                var stepDescription = stepMatch.Groups.Values.Skip(1).First(x => !string.IsNullOrEmpty(x.Value)).Value;
                parsedTest.Steps.Add(CreateTestStep(stepDescription));
            }

            return parsedTest.Title != null && parsedTest.TestCaseId != null ? parsedTest : null;
        }

        // Helper to extract single values using regex
        private static string ExtractValue(string content, string pattern)
        {
            var match = Regex.Match(content, pattern);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        // Helper to classify test steps
        private static TestStep CreateTestStep(string stepDescription)
        {
            if (stepDescription.StartsWith("Verify", StringComparison.OrdinalIgnoreCase) || stepDescription.StartsWith("Confirm", StringComparison.OrdinalIgnoreCase))
            {
                return new TestStep { Expected = stepDescription, Action = string.Empty };
            }
            else
            {
                return new TestStep { Action = stepDescription, Expected = string.Empty };
            }
        }
    }
}
