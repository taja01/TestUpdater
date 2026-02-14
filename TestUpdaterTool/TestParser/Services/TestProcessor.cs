using Microsoft.Extensions.Logging;
using TestParser.Contracts;
using TestParser.Models;

namespace TestParser.Services
{
    public class TestProcessor(IFileHandler fileHandler, ITestCaseParser parser, ILogger<TestProcessor> logger) : ITestProcessor
    {
        public async Task<List<ParsedTest>> ProcessFilesAsync(string folderPath, CancellationToken cancellationToken = default)
        {
            var result = new List<ParsedTest>();

            await foreach (var file in fileHandler.GetFilesAsync(folderPath, parser.FilePattern, cancellationToken))
            {
                var parsedTests = await ProcessFileAsync(file, cancellationToken);
                result.AddRange(parsedTests);
            }

            return result;
        }

        private async Task<List<ParsedTest>> ProcessFileAsync(string filePath, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing file: {filePath}", filePath);

            var parsedTests = parser.ParseFile(filePath);

            DisplayParsedTests(parsedTests);
            return parsedTests;
        }

        private void DisplayParsedTests(IEnumerable<ParsedTest> tests)
        {
            foreach (var test in tests)
            {
                DisplayParsedTest(test);
            }
        }

        private void DisplayParsedTest(ParsedTest parsedTest)
        {
            logger.LogDebug("Title: {Title}", parsedTest.Title);
            logger.LogDebug("TestCaseId: {TestCaseId}", parsedTest.TestCaseId);

            foreach (var step in parsedTest.Steps)
            {
                logger.LogDebug("  Action: {Action}", step.Action);
                logger.LogDebug("  Expected: {Expected}", step.Expected);
            }

            logger.LogDebug("{Separator} END OF TEST CASE {Separator}",
                new string('-', 10),
                new string('-', 10));
        }
    }
}
