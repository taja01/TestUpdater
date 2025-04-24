using TestParser.Models;

namespace TestParser.Contracts
{
    public interface ITestProcessor
    {
        List<ParsedTest> ProcessFiles(string folderPath);
    }
}