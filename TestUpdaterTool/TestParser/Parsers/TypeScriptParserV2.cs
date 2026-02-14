using System.Text.RegularExpressions;
using TestParser.Contracts;
using TestParser.Models;

namespace TestParser.Parsers
{
    public class TypeScriptParserV2(IFileHandler fileHandler) : TypeScriptParserBase, ITestCaseParser
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

        public async Task<List<ParsedTest>> ParseFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var parsedTests = new List<ParsedTest>();
            var fileContent = await fileHandler.ReadFileContentAsync(filePath, cancellationToken);
            var testBlocks = ExtractTestCases(fileContent);

            foreach (var testBlock in testBlocks)
            {
                cancellationToken.ThrowIfCancellationRequested();
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
