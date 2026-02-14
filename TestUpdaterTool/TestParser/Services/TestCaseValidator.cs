using TestParser.Contracts;
using TestParser.Models;

namespace TestParser.Services
{
    public class TestCaseValidator : ITestCaseValidator
    {
        public bool IsValid(ParsedTest testCase, out IEnumerable<string> validationErrors)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(testCase.Title))
            {
                errors.Add("Title is missing or empty.");
            }

            if (!testCase.TestCaseId.HasValue)
            {
                errors.Add("TestCase ID is null.");
            }

            if (testCase.TestCaseId.HasValue && testCase.TestCaseId.Value <= 0)
            {
                errors.Add($"TestCase ID must be a positive integer. Current value: {testCase.TestCaseId.Value}");
            }

            if (testCase.Steps == null || testCase.Steps.Count == 0)
            {
                errors.Add("Test case has no steps defined.");
            }

            validationErrors = errors;
            return errors.Count == 0;
        }
    }
}