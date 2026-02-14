using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TestCaseManager.Configurations;
using TestCaseManager.Contracts;
using TestCaseManager.Services;
using TestParser.Contracts;
using TestParser.Parsers;
using TestParser.Services;
using TestParser.Utilities;

namespace TestRunner
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build()
                )
                .CreateLogger();

            try
            {
                Log.Information("Starting application...");

                var host = Host.CreateDefaultBuilder(args)
                    .UseSerilog()
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.SetBasePath(Directory.GetCurrentDirectory());
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                        config.AddEnvironmentVariables();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        // Configure AzureDevOps options
                        services.Configure<AzureOptions>(context.Configuration.GetSection("AzureOptions"));

                        var azureOptions = context.Configuration.GetSection("AzureOptions").Get<AzureOptions>();
                        var pat = context.Configuration["AzureOptions:PersonalAccessToken"];
                        if (string.IsNullOrWhiteSpace(pat))
                        {
                            throw new InvalidOperationException("Azure Personal Access Token not configured. Use user-secrets, env var or Key Vault.");
                        }

                        // Register HttpClientFactory for AzureDevOpsService
                        services.AddHttpClient<AzureDevOpsService>();

                        // Register application services
                        services.AddSingleton<IFileHandler, FileHandler>();
                        services.AddSingleton<ITestProcessor, TestProcessor>();
                        services.AddSingleton<ITestCaseValidator, TestCaseValidator>();

                        // Test services
                        services.AddSingleton<ITestUpdateService, AzureDevOpsService>();

                        // Parsers
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
