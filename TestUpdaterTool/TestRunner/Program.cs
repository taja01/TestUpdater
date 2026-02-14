using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Serilog;
using System.Text;
using TestCaseManager.Configurations;
using TestCaseManager.Contracts;
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
                        // Configure AzureDevOps options with validation
                        services.AddOptions<AzureOptions>()
                            .Bind(context.Configuration.GetSection("AzureOptions"))
                            .ValidateDataAnnotations()
                            .ValidateOnStart();

                        // Configure TestRunner options
                        services.AddOptions<TestRunnerOptions>()
                            .Bind(context.Configuration.GetSection("TestRunnerOptions"))
                            .ValidateDataAnnotations()
                            .ValidateOnStart();

                        // Register HttpClient-based service - lifetime managed by AddHttpClient
                        services.AddHttpClient<ITestUpdateService, AzureDevOpsService>((serviceProvider, client) =>
                        {
                            var config = serviceProvider.GetRequiredService<IOptions<AzureOptions>>().Value;

                            client.BaseAddress = new Uri($"https://dev.azure.com/{config.Organization}/{config.Project}/_apis/");

                            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{config.PersonalAccessToken}"));
                            client.DefaultRequestHeaders.Authorization =
                                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
                        })
                        .AddTransientHttpErrorPolicy(policy =>
                            policy.WaitAndRetryAsync(3, retryAttempt =>
                                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30)));

                        // Register application services
                        services.AddTransient<IFileHandler, FileHandler>();
                        services.AddTransient<ITestProcessor, TestProcessor>();
                        services.AddTransient<ITestCaseValidator, TestCaseValidator>();

                        // Parsers can be Singleton (stateless)
                        services.AddSingleton<ITestCaseParser, ReqnRollParser>();

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
