using System.ComponentModel.DataAnnotations;

namespace TestRunner.Configurations
{
    public class TestRunnerOptions
    {
        [Required]
        public string TestFilesPath { get; set; } = string.Empty;

        public bool UseRelativePath { get; set; } = true;
    }
}