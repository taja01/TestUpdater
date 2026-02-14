using Common;
using System.ComponentModel.DataAnnotations;

namespace TestParser.Models
{
    public class ParsedTest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public int? TestCaseId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one test step is required")]
        public List<TestStep> Steps { get; set; } = [];
    }
}
