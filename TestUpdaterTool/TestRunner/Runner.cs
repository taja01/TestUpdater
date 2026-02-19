using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestCaseManager.Contracts;
using TestParser.Contracts;
using TestRunner.Configurations;

namespace TestRunner
{
    internal class Runner(
        ITestProcessor testProcessor,
        ITestUpdateService updater,
        ITestCaseValidator validator,
        IOptions<TestRunnerOptions> runnerOptions,
        ILogger<Runner> logger) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Runner is starting the service.");

            var options = runnerOptions.Value;
            string path = options.UseRelativePath
                ? Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\", options.TestFilesPath))
                : options.TestFilesPath;

            logger.LogInformation("Processing test files from: {Path}", path);

            var testCases = await testProcessor.ProcessFilesAsync(path, cancellationToken);

            int successCount = 0;
            int failedCount = 0;

            foreach (var testCase in testCases)
            {
                if (!validator.IsValid(testCase, out var validationErrors))
                {
                    logger.LogWarning("Test case '{Title}' validation failed: {Errors}",
                        testCase.Title ?? "Unknown",
                        string.Join(", ", validationErrors));
                    failedCount++;
                    continue;
                }

                try
                {
                    await updater.UpdateTestCaseStepsAsync(
                        testCase.TestCaseId!.Value,
                        testCase.Steps,
                        testCase.Title!,
                        testCase.Tags.Count > 0 ? testCase.Tags : null,
                        cancellationToken);

                    successCount++;
                    logger.LogInformation("Successfully updated test case '{Title}' (ID: {TestCaseId}) with {TagCount} tags",
                        testCase.Title,
                        testCase.TestCaseId,
                        testCase.Tags.Count);
                }
                catch (HttpRequestException httpEx)
                {
                    logger.LogError(httpEx, "HTTP error updating test case '{Title}' (ID: {TestCaseId}): {Message}",
                        testCase.Title, testCase.TestCaseId, httpEx.Message);
                    failedCount++;
                }
                catch (TaskCanceledException)
                {
                    logger.LogWarning("Update cancelled for test case '{Title}' (ID: {TestCaseId})",
                        testCase.Title, testCase.TestCaseId);
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error updating test case '{Title}' (ID: {TestCaseId})",
                        testCase.Title, testCase.TestCaseId);
                    failedCount++;
                }
            }

            logger.LogInformation("Runner completed processing. Success: {SuccessCount}, Failed: {FailedCount}",
                successCount,
                failedCount);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Runner is stopping the service.");
            return Task.CompletedTask;
        }
    }
}
