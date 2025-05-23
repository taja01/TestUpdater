﻿namespace TestParser.Contracts
{
    public interface IFileHandler
    {
        IEnumerable<string> GetFiles(string folderPath, string searchPattern);
        string ReadFileContent(string filePath);
    }
}
