﻿
using Common;
using Gherkin;
using Gherkin.Ast;
using TestParser.Contracts;
using TestParser.Models;
using TestParser.Utilities;

namespace TestParser.Parsers
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

        private static List<ParsedTest> ProcessGherkinDocument(GherkinDocument gherkinDocument)
        {
            var parsedTests = new List<ParsedTest>();

            // Parse Background steps
            var backgroundSteps = ParseBackgroundSteps(gherkinDocument);

            // Parse Scenarios
            foreach (var scenarioRaw in gherkinDocument.Feature.Children.Where(x => x is Scenario))
            {
                if (scenarioRaw is Scenario scenario)
                {
                    var parsedScenario = ParseScenario(scenario, backgroundSteps);
                    parsedTests.Add(parsedScenario);
                }
            }

            return parsedTests;
        }

        private static List<TestStep> ParseBackgroundSteps(GherkinDocument gherkinDocument)
        {
            var backgroundSteps = new List<TestStep>();

            // Extract Background steps if present in the document

            if (gherkinDocument.Feature.Children
                .FirstOrDefault(child => child is Background) is Background backgroundRaw)
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

        private static ParsedTest ParseScenario(Scenario scenario, List<TestStep> backgroundSteps)
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
                        TestStepHelper.AddActionStep(step.Text, parsedScenario);
                        break;

                    case StepKeywordType.Outcome:
                        mainStepKeywordType = StepKeywordType.Outcome;
                        TestStepHelper.AddValidationStep(step.Text, parsedScenario);
                        break;

                    default:
                        // Use previous keyword type (Action or Outcome) to determine step type
                        if (mainStepKeywordType == StepKeywordType.Outcome)
                        {
                            TestStepHelper.AddValidationStep(step.Text, parsedScenario);
                        }
                        else
                        {
                            TestStepHelper.AddActionStep(step.Text, parsedScenario);
                        }
                        break;
                }
            }
        }
    }
}
