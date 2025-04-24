
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

            return processGherkinDocument(gherkinDocument);
        }

        private List<ParsedTest> processGherkinDocument(Gherkin.Ast.GherkinDocument gherkinDocument)
        {
            var parsedTests = new List<ParsedTest>();

            var backgroundSteps = new ParsedTest();

            var backgroundRaw = gherkinDocument.Feature.Children.FirstOrDefault(x => x is Gherkin.Ast.Background);
            if (backgroundRaw != null)
            {
                var background = backgroundRaw as Background;
                foreach (var backgroundStep in background.Steps)
                {
                    backgroundSteps.Steps.Add(new TestStep { Action = backgroundStep.Text, Expected = string.Empty });
                }
            }

            foreach (var scenarioRaw in gherkinDocument.Feature.Children.Where(x => x is Gherkin.Ast.Scenario))
            {
                var scenario = scenarioRaw as Gherkin.Ast.Scenario;
                var parsedScenario = new ParsedTest();
                parsedScenario.Steps.AddRange(backgroundSteps.Steps);

                parsedScenario.Title = scenario.Name;
                var tcTag = scenario.Tags.FirstOrDefault(x => x.Name.StartsWith("@TC", StringComparison.OrdinalIgnoreCase)).Name;

                if (int.TryParse(tcTag[3..], out int value))
                {
                    parsedScenario.TestCaseId = value;
                }

                var mainStepKeywordType = StepKeywordType.Context;

                foreach (var scenarioStep in scenario.Steps)
                {
                    if (scenarioStep.KeywordType == StepKeywordType.Action || scenarioStep.KeywordType == StepKeywordType.Context)
                    {
                        mainStepKeywordType = StepKeywordType.Action;
                        AddActionStep(scenarioStep.Text, parsedScenario);
                    }
                    else if (scenarioStep.KeywordType == StepKeywordType.Outcome)
                    {
                        mainStepKeywordType = StepKeywordType.Outcome;
                        AddValidationStep(scenarioStep.Text, parsedScenario);
                    }
                    else
                    {
                        if (mainStepKeywordType == StepKeywordType.Outcome)
                        {
                            AddValidationStep(scenarioStep.Text, parsedScenario);
                        }
                        else
                        {
                            AddActionStep(scenarioStep.Text, parsedScenario);
                        }
                    }
                }
                parsedTests.Add(parsedScenario);
            }

            return parsedTests;
        }

        private void AddValidationStep(string text, ParsedTest parsedScenario)
        {
            if (string.IsNullOrEmpty(parsedScenario.Steps.Last().Expected))
            {
                parsedScenario.Steps.Last().Expected = text;
            }
            else
            {
                parsedScenario.Steps.Add(new TestStep { Expected = text });
            }
        }

        private void AddActionStep(string text, ParsedTest parsedScenario)
        {
            if (string.IsNullOrEmpty(parsedScenario.Steps.Last().Action))
            {
                parsedScenario.Steps.Last().Action = text;
            }
            else
            {
                parsedScenario.Steps.Add(new TestStep { Action = text });
            }
        }
    }
}
