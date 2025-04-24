using Common;
using System.Text.RegularExpressions;
using TestParser.Models;
using TestParser.Utilities;

namespace TestParser.Parsers
{
    public class TypeScriptParserBase
    {
        private static readonly Regex TestStepRegex = new(
      @"test\.step\(['\""](.+?)['\""], async \(\) => \{",
      RegexOptions.Singleline
      );

        // Parse a single test block
        protected static ParsedTest? ParseTestBlock(string testBlock)
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
                CreateTestStep(stepDescription, parsedTest);
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
        private static void CreateTestStep(string stepDescription, ParsedTest parsedTest)
        {
            if (stepDescription.StartsWith("Verify", StringComparison.OrdinalIgnoreCase) || stepDescription.StartsWith("Confirm", StringComparison.OrdinalIgnoreCase))
            {
                TestStepHelper.AddValidationStep(stepDescription, parsedTest);
            }
            else
            {
                TestStepHelper.AddActionStep(stepDescription, parsedTest);
            }
        }
    }
}