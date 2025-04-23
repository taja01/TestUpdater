using Common;

namespace TestCaseUpdater
{
    public interface ITestUpdateService
    {
        Task UpdateTestCaseStepsAsync(int testCaseId, List<TestStep> testSteps, string title);
    }
}
