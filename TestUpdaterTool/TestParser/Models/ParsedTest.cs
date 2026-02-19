using Common;
using System.ComponentModel.DataAnnotations;

namespace TestParser.Models
{
    public class ParsedTest
    {
        /// <summary>
        /// Test case title. This is required and cannot be empty.
        /// </summary>
        [Required]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier for the associated test case.
        /// </summary>
        /// <remarks>This property can be null if no test case is associated. It is typically used to link
        /// a test result to a specific test case in a testing framework.</remarks>
        public int? TestCaseId { get; set; }

        /// <summary>
        /// List of test steps, where each step includes an action and an expected result. This is required and must contain at least one step.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one test step is required")]
        public List<TestStep> Steps { get; set; } = [];

        /// <summary>
        /// Tags to be added to the test case (e.g., "Smoke", "Regression", "UI")
        /// </summary>
        public List<string> Tags { get; set; } = [];
    }
}
