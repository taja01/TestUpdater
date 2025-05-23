﻿using Microsoft.Extensions.Logging;
using TestParser.Contracts;
using TestParser.Models;

namespace TestParser.Services
{
    public class TestProcessor(IFileHandler fileHandler, ITestCaseParser parser, ILogger<TestProcessor> logger) : ITestProcessor
    {
        public List<ParsedTest> ProcessFiles(string folderPath)
        {
            var files = fileHandler.GetFiles(folderPath, parser.FilePattern);
            var result = new List<ParsedTest>();

            foreach (var file in files)
            {
                result.AddRange(ProcessFile(file));
            }

            return result;
        }

        private List<ParsedTest> ProcessFile(string filePath)
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
