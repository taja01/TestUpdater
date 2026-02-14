using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using System.Text;
using TestCaseManager.Configurations;
using TestCaseManager.Contracts;

namespace TestCaseManager.Services
{
    public static class AzureDevOpsServiceExtension
    {
        public static IServiceCollection AddAzureDevOpsServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure AzureDevOps options with validation
            services.AddOptions<AzureOptions>()
                .Bind(configuration.GetSection(nameof(AzureOptions)))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Register HttpClient with policies
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

            return services;
        }
    }
}
