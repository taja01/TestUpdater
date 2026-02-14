using TestParser.Contracts;

namespace TestParser.Utilities
{
    public class FileHandler : IFileHandler
    {
        public async IAsyncEnumerable<string> GetFilesAsync(string folderPath, string searchPattern, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var files = Directory.EnumerateFiles(folderPath, searchPattern);

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return file;
                await Task.Yield();
            }
        }

        public async Task<string> ReadFileContentAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return await File.ReadAllTextAsync(filePath, cancellationToken);
        }
    }
}
