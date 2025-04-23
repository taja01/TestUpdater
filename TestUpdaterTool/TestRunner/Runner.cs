using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestCaseUpdater;
using TestParser;

namespace TestRunner
{
    internal class Runner : IHostedService
    {
        private readonly ITestUpdateService _updater;
        private readonly ITestProcessor _testProcessor;
        private readonly ILogger<Runner> _logger;

        public Runner(ITestProcessor testProcessor, ITestUpdateService updater, ILogger<Runner> logger)
        {
            _testProcessor = testProcessor;
            _updater = updater;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Runner is starting the service.");

            string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
            string typeScriptPath = Path.Combine(projectDirectory, "example", "typeScript");
            var testCases = _testProcessor.ProcessFiles(typeScriptPath);

            foreach (var testCase in testCases)
            {
                if (!IsValidTestCase(testCase))
                {
                    continue;
                }

                // Update test case steps
                await _updater.UpdateTestCaseStepsAsync(testCase.TestCaseId.Value, testCase.Steps, testCase.Title);
            }

            _logger.LogInformation("Runner has completed processing.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Runner is stopping the service.");
            // If your service requires any clean-up, put it here.
            return Task.CompletedTask;
        }

        private bool IsValidTestCase(ParsedTest testCase)
        {
            if (string.IsNullOrEmpty(testCase.Title))
            {
                _logger.LogWarning("Title missing!");
                return false;
            }

            if (!testCase.TestCaseId.HasValue)
            {
                _logger.LogWarning("TestCase ID is null!");
                return false;
            }

            return true;
        }
    }
}
