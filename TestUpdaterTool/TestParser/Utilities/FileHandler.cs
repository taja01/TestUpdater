using TestParser.Contracts;

namespace TestParser.Utilities
{
    // Handles file operations
    public class FileHandler : IFileHandler
    {
        public IEnumerable<string> GetFiles(string folderPath, string searchPattern)
        {
            return Directory.GetFiles(folderPath, searchPattern);
        }

        public string ReadFileContent(string filePath)
        {
            return File.ReadAllText(filePath);
        }
    }
}
