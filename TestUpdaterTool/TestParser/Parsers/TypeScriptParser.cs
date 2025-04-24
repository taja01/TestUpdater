using System.Text.RegularExpressions;
using TestParser.Contracts;
using TestParser.Models;

namespace TestParser.Parsers
{
    public class TypeScriptParser(IFileHandler fileHandler) : TypeScriptParserBase, ITestCaseParser
    {
        public string FilePattern => "*.system.spec.ts";
        private static readonly Regex TestBlockRegex = new(
           @"test\s*\(\s*\{(?:[^{}]*|\{(?:[^{}]*|\{.*?\})*\})*\}\s*,\s*\{(?:[^{}]*|\{(?:[^{}]*|\{.*?\})*\})*\}\s*,\s*async\s*\([^)]*\)\s*=>\s*\{(?:[^{}]*|\{(?:[^{}]*|\{.*?\})*\})*\}\s*\)",
            RegexOptions.Singleline
        );

        // Parse the content of a file
        public List<ParsedTest> ParseFile(string filePath)
        {
            var parsedTests = new List<ParsedTest>();
            var fileContent = fileHandler.ReadFileContent(filePath);
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
