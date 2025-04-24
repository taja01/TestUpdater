using Common;

namespace TestCaseManager.Contracts
{
    public interface ITestUpdateService
    {
        Task UpdateTestCaseStepsAsync(int testCaseId, List<TestStep> testSteps, string title);
    }
}
