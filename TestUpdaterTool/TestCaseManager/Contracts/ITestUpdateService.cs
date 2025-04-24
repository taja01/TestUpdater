using Common;

namespace TestSyncTool.Contracts
{
    public interface ITestUpdateService
    {
        Task UpdateTestCaseStepsAsync(int testCaseId, List<TestStep> testSteps, string title);
    }
}
