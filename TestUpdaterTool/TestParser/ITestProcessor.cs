namespace TestParser
{
    public interface ITestProcessor
    {
        List<ParsedTest> ProcessFiles(string folderPath);
    }
}