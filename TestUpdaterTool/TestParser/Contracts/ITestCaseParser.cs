using TestParser.Models;

namespace TestParser.Contracts
{
    public interface ITestCaseParser
    {
        string FilePattern { get; }

        Task<List<ParsedTest>> ParseFileAsync(string path, CancellationToken cancellationToken = default);
    }
}
