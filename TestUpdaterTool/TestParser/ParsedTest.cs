using Common;

namespace TestParser
{
    public class ParsedTest
    {
        public string? Title { get; set; }
        public int? TestCaseId { get; set; }
        public List<TestStep> Steps { get; set; } = [];
    }
}
