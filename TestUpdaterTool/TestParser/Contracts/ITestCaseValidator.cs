using TestParser.Models;

namespace TestParser.Contracts
{
    public interface ITestCaseValidator
    {
        bool IsValid(ParsedTest testCase, out IEnumerable<string> validationErrors);
    }
}