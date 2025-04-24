using Common;
using TestParser.Models;

namespace TestParser.Utilities
{
    public static class TestStepHelper
    {
        /// <summary>
        /// Adds a validation step to the existing list of steps in a ParsedTest.
        /// </summary>
        public static void AddValidationStep(string text, ParsedTest currentTestScenario)
        {
            // If the last step has no Expected value, add it to the last step
            if (currentTestScenario.Steps.Count > 0 && string.IsNullOrEmpty(currentTestScenario.Steps.Last().Expected))
            {
                currentTestScenario.Steps.Last().Expected = text;
            }
            else
            {
                // Otherwise, add a new step with the expected value
                currentTestScenario.Steps.Add(new TestStep { Expected = text });
            }
        }

        /// <summary>
        /// Adds an action step to the existing list of steps in a ParsedTest.
        /// </summary>
        public static void AddActionStep(string text, ParsedTest currentTestScenario)
        {
            // If the last step has no Action value, add it to the last step
            if (currentTestScenario.Steps.Count > 0 && string.IsNullOrEmpty(currentTestScenario.Steps.Last().Action))
            {
                currentTestScenario.Steps.Last().Action = text;
            }
            else
            {
                // Otherwise, add a new step with the action value
                currentTestScenario.Steps.Add(new TestStep { Action = text });
            }
        }
    }
}
