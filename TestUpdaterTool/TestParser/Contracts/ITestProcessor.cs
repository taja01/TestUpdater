using TestParser.Models;

namespace TestParser.Contracts
{
    public interface ITestProcessor
    {
        Task<List<ParsedTest>> ProcessFilesAsync(string folderPath, CancellationToken cancellationToken = default);
    }
}