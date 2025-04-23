using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TestCaseUpdater;
using TestParser;

namespace TestRunner
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure Serilog
            ////Log.Logger = new LoggerConfiguration()
            ////    .MinimumLevel.Debug() // Set default log level
            ////    .ReadFrom.Configuration(new ConfigurationBuilder()
            ////        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Read configuration from appsettings.json
            ////        .AddEnvironmentVariables()
            ////        .Build()
            ////    )
            ////    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")

            ////    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            ////    .CreateLogger();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Read configuration from appsettings.json
                .AddEnvironmentVariables()
                .Build()
                )
                .CreateLogger();
            // Build and run the host
            try
            {
                Log.Information("Starting application...");

                var host = Host.CreateDefaultBuilder(args)
                    .UseSerilog() // Replace default logging with Serilog
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        // Configuration sources
                        config.SetBasePath(Directory.GetCurrentDirectory());
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                        config.AddEnvironmentVariables();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        // Configure AzureDevOps options
                        services.Configure<AzureOptions>(context.Configuration.GetSection("AzureOptions"));

                        // Register HttpClientFactory for AzureDevOpsService
                        services.AddHttpClient<AzureDevOpsService>();

                        // Register application services
                        services.AddSingleton<IFileHandler, FileHandler>();
                        services.AddSingleton<ITestUpdateService, AzureDevOpsService>();
                        services.AddSingleton<ITestCaseParser, TypeScriptParserV2>();
                        services.AddSingleton<ITestProcessor, TestProcessor>();

                        // Add the runner as a hosted service
                        services.AddHostedService<Runner>();
                    })
                    .Build();

                // Run the host
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly.");
            }
            finally
            {
                Log.CloseAndFlush(); // Ensure all logs are written before application exits
            }
        }
    }
}
