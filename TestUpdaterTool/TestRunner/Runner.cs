using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestCaseManager.Contracts;
using TestParser.Contracts;
using TestParser.Models;

namespace TestRunner
{
    internal class Runner(ITestProcessor testProcessor, ITestUpdateService updater, ILogger<Runner> logger) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Runner is starting the service.");

            string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
            string path = Path.Combine(projectDirectory, "example", "reqnroll"); //reqnroll typeScript
            var testCases = testProcessor.ProcessFiles(path);

            foreach (var testCase in testCases)
            {
                if (!IsValidTestCase(testCase))
                {
                    continue;
                }

                // Update test case steps
                await updater.UpdateTestCaseStepsAsync(testCase.TestCaseId!.Value, testCase.Steps, testCase.Title!);
            }

            logger.LogInformation("Runner has completed processing.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Runner is stopping the service.");
            // If your service requires any clean-up, put it here.
            return Task.CompletedTask;
        }

        private bool IsValidTestCase(ParsedTest testCase)
        {
            if (string.IsNullOrEmpty(testCase.Title))
            {
                logger.LogWarning("Title missing!");
                return false;
            }

            if (!testCase.TestCaseId.HasValue)
            {
                logger.LogWarning("TestCase ID is null!");
                return false;
            }

            return true;
        }
    }
}
