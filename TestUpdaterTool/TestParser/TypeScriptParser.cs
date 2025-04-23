using System.Text.RegularExpressions;

namespace TestParser
{
    public class TypeScriptParser : TypeScriptParserBase, ITestCaseParser
    {
        public string FilePattern => "*.system.spec.ts";
        private static readonly Regex TestBlockRegex = new(
           @"test\s*\(\s*\{(?:[^{}]*|\{(?:[^{}]*|\{.*?\})*\})*\}\s*,\s*\{(?:[^{}]*|\{(?:[^{}]*|\{.*?\})*\})*\}\s*,\s*async\s*\([^)]*\)\s*=>\s*\{(?:[^{}]*|\{(?:[^{}]*|\{.*?\})*\})*\}\s*\)",
            RegexOptions.Singleline
        );

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
    }
}
