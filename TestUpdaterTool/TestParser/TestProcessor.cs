namespace TestParser
{
    // Handles business logic for processing files
    public class TestProcessor(IFileHandler fileHandler, ITestCaseParser parser)
    {
        private readonly IFileHandler _fileHandler = fileHandler;
        private readonly ITestCaseParser _parser = parser;

        public List<ParsedTest> ProcessFiles(string folderPath)
        {
            var files = _fileHandler.GetFiles(folderPath, _parser.FilePattern);
            var result = new List<ParsedTest>();

            foreach (var file in files)
            {
                result.AddRange(ProcessFile(file));
            }

            return result;
        }

        private List<ParsedTest> ProcessFile(string filePath)
        {
            Console.WriteLine($"Processing file: {filePath}");
            var content = _fileHandler.ReadFileContent(filePath);
            var parsedTests = _parser.ParseFile(content);

            DisplayParsedTests(parsedTests); // Optional logging/display functionality
            return parsedTests;
        }

        private static void DisplayParsedTests(IEnumerable<ParsedTest> tests)
        {
            foreach (var test in tests)
            {
                DisplayParsedTest(test);
            }
        }

        private static void DisplayParsedTest(ParsedTest parsedTest)
        {
            Console.WriteLine($"Title: {parsedTest.Title}");
            Console.WriteLine($"TestCaseId: {parsedTest.TestCaseId}");
            foreach (var step in parsedTest.Steps)
            {
                Console.WriteLine($"  Action: {step.Action}");
                Console.WriteLine($"  Expected: {step.Expected}");
            }
            Console.WriteLine(new string('-', 50));
        }
    }
}
