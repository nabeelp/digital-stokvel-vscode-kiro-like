using DigitalStokvel.Infrastructure.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalStokvel.Infrastructure;

/// <summary>
/// Dependency injection extensions for external services
/// </summary>
public static class ExternalServicesDependencyInjection
{
    /// <summary>
    /// Add CBS client with resilience policies
    /// </summary>
    public static IServiceCollection AddCbsClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind options
        services.Configure<CbsClientOptions>(
            configuration.GetSection(CbsClientOptions.SectionName));

        // Register HttpClient with typed client
        services.AddHttpClient<ICbsClient, CbsClient>((serviceProvider, client) =>
        {
            var options = configuration
                .GetSection(CbsClientOptions.SectionName)
                .Get<CbsClientOptions>();

            if (options == null || string.IsNullOrEmpty(options.BaseUrl))
            {
                throw new InvalidOperationException(
                    $"CbsClient configuration is missing. Ensure '{CbsClientOptions.SectionName}' section is configured in appsettings.json");
            }

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            // Add API key header if configured
            if (!string.IsNullOrEmpty(options.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-API-Key", options.ApiKey);
            }

            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }

    /// <summary>
    /// Add Payment Gateway client
    /// </summary>
    public static IServiceCollection AddPaymentGatewayClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: Implement in Task 4.2.1
        // services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>(...)
        
        return services;
    }

    /// <summary>
    /// Add SMS Gateway client
    /// </summary>
    public static IServiceCollection AddSmsGatewayClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: Implement in Task 4.3.1
        // services.AddHttpClient<ISmsGatewayClient, SmsGatewayClient>(...)
        
        return services;
    }
}
