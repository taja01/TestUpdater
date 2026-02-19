using System.Text.RegularExpressions;
using TestParser.Models;
using TestParser.Utilities;

namespace TestParser.Parsers
{
    public partial class TypeScriptParserBase
    {
        [GeneratedRegex(@"test\.step\(['\""](.+?)['\""], async \(\) => \{", RegexOptions.Singleline)]
        private static partial Regex GetTestStepRegex();

        [GeneratedRegex(@"tag:\s*\[([^\]]+)\]", RegexOptions.Singleline)]
        private static partial Regex GetTagRegex();

        [GeneratedRegex(@"['\""]@?([^'\"",\]]+)['\""]", RegexOptions.None)]
        private static partial Regex GetIndividualTagRegex();

        private static Regex TestStepRegex => GetTestStepRegex();
        private static Regex TagRegex => GetTagRegex();
        private static Regex IndividualTagRegex => GetIndividualTagRegex();

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

            // Extract tags
            parsedTest.Tags = ExtractTags(testBlock);

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

        // Helper to extract tags from TypeScript array format
        private static List<string> ExtractTags(string testBlock)
        {
            var tags = new List<string>();

            // Match: tag: ['@smoke', '@login']
            var tagArrayMatch = TagRegex.Match(testBlock);
            if (!tagArrayMatch.Success)
            {
                return tags;
            }

            // Extract the content inside the brackets
            var tagArrayContent = tagArrayMatch.Groups[1].Value;

            // Extract individual tags from quotes
            var individualTagMatches = IndividualTagRegex.Matches(tagArrayContent);
            foreach (Match tagMatch in individualTagMatches)
            {
                var tag = tagMatch.Groups[1].Value.Trim();
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    tags.Add(tag);
                }
            }

            return tags;
        }

        // Helper to classify test steps
        private static void CreateTestStep(string stepDescription, ParsedTest parsedTest)
        {
            if (stepDescription.StartsWith("Verify", StringComparison.OrdinalIgnoreCase) ||
                stepDescription.StartsWith("Confirm", StringComparison.OrdinalIgnoreCase))
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