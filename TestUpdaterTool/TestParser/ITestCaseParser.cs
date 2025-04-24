namespace TestParser
{
    public interface ITestCaseParser
    {
        string FilePattern { get; }
        List<ParsedTest> ParseFile(string path);
    }
}
