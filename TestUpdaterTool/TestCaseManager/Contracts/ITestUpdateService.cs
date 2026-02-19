using Common;

namespace TestCaseManager.Contracts
{
    public interface ITestUpdateService
    {
        /// <summary>
        /// Asynchronously updates the steps, title, and tags of a specified test case.
        /// </summary>
        /// <remarks>Throws an exception if the test case ID is invalid or if the update operation fails.
        /// The method does not validate the content of test steps or tags beyond null checks.</remarks>
        /// <param name="testCaseId">The unique identifier of the test case to update. Must correspond to an existing test case.</param>
        /// <param name="testSteps">A list of test steps that define the actions to be performed in the test case. Cannot be null.</param>
        /// <param name="title">The new title for the test case, providing a brief description of its purpose. Cannot be null or empty.</param>
        /// <param name="tags">An optional list of tags to associate with the test case for categorization or filtering. May be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous update operation. The task completes when the update is finished.</returns>
        Task UpdateTestCaseStepsAsync(int testCaseId, List<TestStep> testSteps, string title, List<string>? tags = null, CancellationToken cancellationToken = default);
    }
}
