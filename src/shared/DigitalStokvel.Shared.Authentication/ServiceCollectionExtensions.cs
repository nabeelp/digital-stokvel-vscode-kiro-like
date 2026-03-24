using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalStokvel.Shared.Authentication;

/// <summary>
/// Extension methods for registering authentication services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add JWT token generation and validation services
    /// </summary>
    public static IServiceCollection AddJwtTokenService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        return services;
    }
}
