using System.Text.RegularExpressions;

namespace TestParser
{
    public class TypeScriptParserV2 : TypeScriptParserBase, ITestCaseParser
    {
        public string FilePattern => "*.system.spec.ts";

        private static List<string> ExtractTestCases(string fileContent)
        {
            string pattern = @"(?=^(.*test\())";
            var segments = Regex.Split(fileContent, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase)
                .Skip(2)
                .Where((item, index) => index % 2 == 0);

            return [.. segments];
        }

        public List<ParsedTest> ParseFile(string fileContent)
        {
            var parsedTests = new List<ParsedTest>();

            var testBlocks = ExtractTestCases(fileContent);
            foreach (var testBlock in testBlocks)
            {
                var parsedTest = ParseTestBlock(testBlock);
                if (parsedTest != null)
                {
                    parsedTests.Add(parsedTest);
                }
            }

            return parsedTests;
        }
    }
}
