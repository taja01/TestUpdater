using TestParser.Models;

namespace TestParser.Contracts
{
    public interface ITestCaseParser
    {
        string FilePattern { get; }
        List<ParsedTest> ParseFile(string path);
    }
}
