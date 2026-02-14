namespace TestParser.Contracts
{
    public interface IFileHandler
    {
        IAsyncEnumerable<string> GetFilesAsync(string folderPath, string searchPattern, CancellationToken cancellationToken = default);

        Task<string> ReadFileContentAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
