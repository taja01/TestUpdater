using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TestCaseManager.Services;
using TestParser.Contracts;
using TestParser.Parsers;
using TestParser.Services;
using TestParser.Utilities;
using TestRunner.Configurations;

namespace TestRunner
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Build configuration once
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>() // Add user secrets support
                .Build();

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Starting application...");

                var host = Host.CreateDefaultBuilder(args)
                    .UseSerilog()
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        // Clear default config and use our pre-built configuration
                        config.Sources.Clear();
                        config.AddConfiguration(configuration);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        // Configure TestRunner options
                        services.AddOptions<TestRunnerOptions>()
                            .Bind(context.Configuration.GetSection("TestRunnerOptions"))
                            .ValidateDataAnnotations()
                            .ValidateOnStart();

                        // Add Azure DevOps services using extension method
                        services.AddAzureDevOpsServices(context.Configuration);

                        // Register application services
                        services.AddTransient<IFileHandler, FileHandler>();
                        services.AddTransient<ITestProcessor, TestProcessor>();
                        services.AddTransient<ITestCaseValidator, TestCaseValidator>();

                        // Parsers can be Singleton (stateless)
                        services.AddSingleton<ITestCaseParser, TypeScriptParserV2>();

                        // Add the runner as a hosted service
                        services.AddHostedService<Runner>();
                    })
                    .Build();

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
