using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestCaseUpdater;
using TestParser;

namespace TestRunner
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Create a new HostBuilder
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Optionally set BasePath and specify the json file(s)
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    // Also add environment variables or other configuration sources if needed.
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    // Bind AzureOptions to the "AzureOptions" section of appsettings.json
                    services.Configure<AzureOptions>(context.Configuration.GetSection("AzureOptions"));

                    // Register your service dependencies.
                    services.AddSingleton<IFileHandler, FileHandler>();
                    services.AddSingleton<ITestUpdateService, AzureDevOpsService>();
                    services.AddSingleton<ITestCaseParser, TypeScriptParserV2>();
                    services.AddSingleton<ITestProcessor, TestProcessor>();

                    // Register the runner as a Hosted Service
                    services.AddHostedService<Runner>();

                    // (Optional) add logging configuration here or use defaults.
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();

            // Run the host. This will call the IHostedService implementations.
            await host.RunAsync();
        }
    }
}
