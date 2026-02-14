using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System.Text;
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
                        services.AddOptions<AzureOptions>()
                        .Bind(context.Configuration.GetSection("AzureOptions"))
                        .ValidateDataAnnotations()
                        .ValidateOnStart();

                        // Register HttpClientFactory for AzureDevOpsService
                        services.AddHttpClient<AzureDevOpsService>((serviceProvider, client) =>
                        {
                            var config = serviceProvider.GetRequiredService<IOptions<AzureOptions>>().Value;

                            client.BaseAddress = new Uri($"https://dev.azure.com/{config.Organization}/{config.Project}/_apis/");

                            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{config.PersonalAccessToken}"));
                            client.DefaultRequestHeaders.Authorization =
                                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
                        });

                        // Register application services
                        // Stateless services - use Transient
                        services.AddTransient<IFileHandler, FileHandler>();
                        services.AddTransient<ITestProcessor, TestProcessor>();
                        services.AddTransient<ITestCaseValidator, TestCaseValidator>();

                        // HttpClient-based service - lifetime managed by AddHttpClient
                        services.AddHttpClient<ITestUpdateService, AzureDevOpsService>();

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
