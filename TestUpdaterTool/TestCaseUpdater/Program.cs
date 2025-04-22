using Common;

namespace TestCaseUpdater
{
    internal class Program
    {
        static async Task Main()
        {
            // Configuration Parameters
            string organization = "TamasJarvas";          // Azure DevOps Organization
            string project = "Sandbox";                    // Azure DevOps Project
            string personalAccessToken = ""; // PAT Token

            // Initialize Azure DevOps Service
            AzureDevOpsService adoService = new(organization, project, personalAccessToken);

            // Define Test Steps
            var testSteps = new List<TestStep> {  new()
                {
                    Action = "Given I am in the main page",
                    Expected = "Main page loaded"
                },
                new()
                {
                    Action = "When I click login button",
                    Expected = "Login popup appears"
                }
            };

            // Test Case ID
            int testCaseId = 14;

            // Update test case steps
            await adoService.UpdateTestCaseStepsAsync(testCaseId, testSteps, "My Test Title 2");

            // Dispose resources
            adoService.Dispose();
        }
    }
}
