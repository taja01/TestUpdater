using Common;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace TestCaseUpdater
{
    public class AzureDevOpsService : ITestUpdateService
    {
        private readonly AzureOptions _options;
        private readonly HttpClient _httpClient;

        public AzureDevOpsService(HttpClient httpClient, IOptions<AzureOptions> options)
        {
            _options = options.Value;

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri($"https://dev.azure.com/{_options.Organization}/{_options.Project}/_apis/");


            // Add authorization header using the PAT
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_options.PersonalAccessToken}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
        }

        /// <summary>
        /// Updates a test case in Azure DevOps with custom test steps.
        /// </summary>
        /// <param name="testCaseId">The ID of the test case to update.</param>
        /// <param name="testSteps">The test steps object to represent actions and expected results.</param>
        /// <param name="title">Test Title</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateTestCaseStepsAsync(int testCaseId, List<TestStep> testSteps, string title)
        {
            if (testSteps == null || testSteps.Count == 0)
            {
                throw new ArgumentException("Test steps cannot be null or empty.");
            }

            string url = $"https://dev.azure.com/{_options.Organization}/{_options.Project}/_apis/wit/workitems/{testCaseId}?api-version=7.1-preview.3";

            // Build test steps string in format understood by Azure DevOps
            var stepsFieldValue = BuildTestStepsValue(testSteps);

            // Prepare patch document
            var patchDocument = new[]
            {
                new { op = "add", path = "/fields/System.Title", value = title},
                new { op = "add", path = "/fields/Microsoft.VSTS.TCM.AutomationStatus", value = "Planned"},
                new { op = "add", path = "/fields/Microsoft.VSTS.TCM.Steps", value = stepsFieldValue }
            };

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(patchDocument), Encoding.UTF8, "application/json-patch+json");
                var response = await _httpClient.PatchAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Test case {testCaseId} updated successfully with steps.");
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error updating test case {testCaseId}: {response.StatusCode}\n{errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while updating test case: {ex.Message}");
            }
        }

        /// <summary>
        /// Converts test steps into a format compatible with Azure DevOps (HTML-style XML).
        /// </summary>
        /// <param name="testSteps">The test steps object.</param>
        /// <returns>A string representation of the test steps in XML format.</returns>
        private static string BuildTestStepsValue(List<TestStep> testSteps)
        {
            var xmlBuilder = new StringBuilder();

            // Add root node with metadata like title and automation status
            xmlBuilder.Append("<steps id=\"0\" last=\"\">");

            // Iterate through test steps and build their XML representation
            for (int i = 0; i < testSteps.Count; i++)
            {
                var step = testSteps[i];

                xmlBuilder.Append($"<step id=\"{i + 1}\" type=\"ActionStep\">");

                // Add the step's action
                xmlBuilder.Append("<parameterizedString isformatted=\"true\">");
                xmlBuilder.Append(System.Security.SecurityElement.Escape(step.Action)); // Escape special XML chars
                xmlBuilder.Append("</parameterizedString>");

                // Add the step's expected result
                xmlBuilder.Append("<parameterizedString isformatted=\"true\">");
                xmlBuilder.Append(System.Security.SecurityElement.Escape(step.Expected)); // Escape special XML chars
                xmlBuilder.Append("</parameterizedString>");

                xmlBuilder.Append("</step>");
            }

            xmlBuilder.Append("</steps>");
            return xmlBuilder.ToString();
        }
    }
}
