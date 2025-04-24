
using Common;
using Gherkin;
using Gherkin.Ast;

namespace TestParser
{
    public class ReqnRollParser : ITestCaseParser
    {
        public string FilePattern => "*.feature";

        public List<ParsedTest> ParseFile(string path)
        {
            var parser = new Parser();
            var gherkinDocument = parser.Parse(path);

            return ProcessGherkinDocument(gherkinDocument);
        }

        private static List<ParsedTest> ProcessGherkinDocument(Gherkin.Ast.GherkinDocument gherkinDocument)
        {
            var parsedTests = new List<ParsedTest>();

            // Parse Background steps
            var backgroundSteps = ParseBackgroundSteps(gherkinDocument);

            // Parse Scenarios
            foreach (var scenarioRaw in gherkinDocument.Feature.Children.Where(x => x is Gherkin.Ast.Scenario))
            {
                if (scenarioRaw is Gherkin.Ast.Scenario scenario)
                {
                    var parsedScenario = ParseScenario(scenario, backgroundSteps);
                    parsedTests.Add(parsedScenario);
                }
            }

            return parsedTests;
        }

        private static List<TestStep> ParseBackgroundSteps(Gherkin.Ast.GherkinDocument gherkinDocument)
        {
            var backgroundSteps = new List<TestStep>();

            // Extract Background steps if present in the document

            if (gherkinDocument.Feature.Children
                .FirstOrDefault(child => child is Gherkin.Ast.Background) is Background backgroundRaw)
            {
                foreach (var step in backgroundRaw.Steps)
                {
                    backgroundSteps.Add(new TestStep
                    {
                        Action = step.Text,
                        Expected = string.Empty
                    });
                }
            }

            return backgroundSteps;
        }

        private static ParsedTest ParseScenario(Gherkin.Ast.Scenario scenario, List<TestStep> backgroundSteps)
        {
            var parsedScenario = new ParsedTest();

            // Add background steps
            parsedScenario.Steps.AddRange(backgroundSteps);

            // Set Title and TestCaseId from Scenario tags
            parsedScenario.Title = scenario.Name;
            parsedScenario.TestCaseId = GetTestCaseIdFromTags(scenario.Tags);

            // Process the steps in the scenario
            ParseScenarioSteps(scenario.Steps, parsedScenario);

            return parsedScenario;
        }

        private static int? GetTestCaseIdFromTags(IEnumerable<Tag> tags)
        {
            // Search for the @TC<id> tag and extract the ID
            var testCaseTag = tags.FirstOrDefault(tag => tag.Name.StartsWith("@TC", StringComparison.OrdinalIgnoreCase));
            if (testCaseTag != null && int.TryParse(testCaseTag.Name[3..], out int testCaseId))
            {
                return testCaseId;
            }
            return null;
        }

        private static void ParseScenarioSteps(IEnumerable<Step> steps, ParsedTest parsedScenario)
        {
            // Keep track of the current main keyword type (Action, Context, or Outcome)
            var mainStepKeywordType = StepKeywordType.Context;

            foreach (var step in steps)
            {
                switch (step.KeywordType)
                {
                    case StepKeywordType.Context:
                    case StepKeywordType.Action:
                        mainStepKeywordType = StepKeywordType.Action;
                        AddActionStep(step.Text, parsedScenario);
                        break;

                    case StepKeywordType.Outcome:
                        mainStepKeywordType = StepKeywordType.Outcome;
                        AddValidationStep(step.Text, parsedScenario);
                        break;

                    default:
                        // Use previous keyword type (Action or Outcome) to determine step type
                        if (mainStepKeywordType == StepKeywordType.Outcome)
                        {
                            AddValidationStep(step.Text, parsedScenario);
                        }
                        else
                        {
                            AddActionStep(step.Text, parsedScenario);
                        }
                        break;
                }
            }
        }

        private static void AddValidationStep(string text, ParsedTest parsedScenario)
        {
            // If the last step has no Expected value, add it to the last step
            if (parsedScenario.Steps.Count > 0 && string.IsNullOrEmpty(parsedScenario.Steps.Last().Expected))
            {
                parsedScenario.Steps.Last().Expected = text;
            }
            else
            {
                // Otherwise, add a new step with the expected value
                parsedScenario.Steps.Add(new TestStep { Expected = text });
            }
        }

        private static void AddActionStep(string text, ParsedTest parsedScenario)
        {
            // If the last step has no Action value, add it to the last step
            if (parsedScenario.Steps.Count > 0 && string.IsNullOrEmpty(parsedScenario.Steps.Last().Action))
            {
                parsedScenario.Steps.Last().Action = text;
            }
            else
            {
                // Otherwise, add a new step with the action value
                parsedScenario.Steps.Add(new TestStep { Action = text });
            }
        }
    }
}
