using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using AspNetCoreRateLimit;

namespace DigitalStokvel.ApiGateway;

public class Program
{
    public static void Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("Starting Digital Stokvel API Gateway");
            
            var builder = WebApplication.CreateBuilder(args);

            // Add Serilog
            builder.Host.UseSerilog();

            // Add services to the container
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline
            ConfigureMiddleware(app);

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "API Gateway failed to start");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Add JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings.GetValue<string>("SecretKey") 
            ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var key = Encoding.UTF8.GetBytes(secretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.GetValue<string>("Issuer") ?? "stokvel-api",
                ValidateAudience = true,
                ValidAudience = jwtSettings.GetValue<string>("Audience") ?? "stokvel-clients",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Log.Warning("JWT authentication failed: {Message}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Log.Debug("JWT token validated for user: {UserId}", 
                        context.Principal?.FindFirst("sub")?.Value);
                    return Task.CompletedTask;
                }
            };
        });

        // Add Authorization with RBAC policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireChairperson", policy => 
                policy.RequireClaim("roles", "chairperson"));
            
            options.AddPolicy("RequireTreasurer", policy => 
                policy.RequireClaim("roles", "treasurer"));
            
            options.AddPolicy("RequireSecretary", policy => 
                policy.RequireClaim("roles", "secretary"));
            
            options.AddPolicy("RequireMember", policy => 
                policy.RequireClaim("roles", "member", "chairperson", "treasurer", "secretary"));
            
            options.AddPolicy("RequireFinancialRole", policy => 
                policy.RequireClaim("roles", "chairperson", "treasurer"));
        });

        // Add Rate Limiting
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<ClientRateLimitOptions>(configuration.GetSection("ClientRateLimiting"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        // Add Health Checks
        services.AddHealthChecks();

        // Add YARP Reverse Proxy
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        // Use CORS
        app.UseCors("AllowAll");

        // Use HTTPS redirection
        app.UseHttpsRedirection();

        // Use Serilog request logging
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
                
                // Log user information if authenticated
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
                    diagnosticContext.Set("Phone", httpContext.User.FindFirst("phone")?.Value);
                }
            };
        });

        // Use Rate Limiting
        app.UseClientRateLimiting();

        // Use Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Map Health Checks
        app.MapHealthChecks("/health");

        // Use YARP Reverse Proxy
        app.MapReverseProxy();
    }
}
