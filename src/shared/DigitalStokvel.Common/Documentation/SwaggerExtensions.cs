using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace DigitalStokvel.Common.Documentation;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI documentation in services.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds Swagger/OpenAPI documentation to the service collection with JWT authentication.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceInfo">Service information for documentation.</param>
    /// <param name="includeXmlComments">Whether to include XML documentation comments (default: true).</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddDigitalStokvelSwagger(
        this IServiceCollection services,
        SwaggerConfiguration.ServiceInfo serviceInfo,
        bool includeXmlComments = true)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(options =>
        {
            // Set OpenAPI document info
            options.SwaggerDoc(serviceInfo.Version, SwaggerConfiguration.CreateOpenApiInfo(serviceInfo));

            // Add JWT Bearer authentication
            var securityScheme = SwaggerConfiguration.CreateJwtSecurityScheme();
            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(SwaggerConfiguration.CreateJwtSecurityRequirement());

            // Include XML comments if enabled
            if (includeXmlComments)
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }

            // Enable annotations for better documentation
            options.EnableAnnotations();

            // Use full names for schema IDs to avoid conflicts
            options.CustomSchemaIds(type => type.FullName);

            // Order actions by relative path
            options.OrderActionsBy(apiDesc => apiDesc.RelativePath);

            // Add standard headers to documentation
            AddStandardHeaderParameters(options);
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger UI middleware for the application.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="serviceInfo">Service information for documentation.</param>
    /// <param name="routePrefix">The route prefix for Swagger UI (default: "swagger").</param>
    /// <returns>The updated application builder.</returns>
    public static IApplicationBuilder UseDigitalStokvelSwagger(
        this IApplicationBuilder app,
        SwaggerConfiguration.ServiceInfo serviceInfo,
        string routePrefix = "swagger")
    {
        // Enable Swagger JSON endpoint
        app.UseSwagger();

        // Enable Swagger UI
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{serviceInfo.Version}/swagger.json", 
                $"{serviceInfo.Name} {serviceInfo.Version.ToUpper()}");
            
            options.RoutePrefix = routePrefix;
            
            // Improve UI experience
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            options.DefaultModelsExpandDepth(2);
            options.DisplayRequestDuration();
            options.EnableFilter();
            options.EnableDeepLinking();
            options.EnableTryItOutByDefault();
            
            // Custom CSS for branding (optional)
            options.InjectStylesheet("/swagger/custom.css");
            
            // Display operation ID for easier testing
            options.DisplayOperationId();
            
            // Show request headers
            options.EnableValidator();
        });

        return app;
    }

    /// <summary>
    /// Adds Swagger middleware with environment-specific configuration.
    /// Only enables Swagger UI in Development and Staging environments for security.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <param name="serviceInfo">Service information for documentation.</param>
    /// <param name="enableInProduction">Whether to enable Swagger in production (default: false).</param>
    /// <returns>The updated application builder.</returns>
    public static IApplicationBuilder UseDigitalStokvelSwaggerWithEnvironment(
        this IApplicationBuilder app,
        IHostEnvironment environment,
        SwaggerConfiguration.ServiceInfo serviceInfo,
        bool enableInProduction = false)
    {
        if (environment.IsDevelopment() || environment.IsStaging() || enableInProduction)
        {
            app.UseDigitalStokvelSwagger(serviceInfo);
        }

        return app;
    }

    /// <summary>
    /// Adds standard API headers to operation documentation.
    /// </summary>
    private static void AddStandardHeaderParameters(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        var headers = SwaggerConfiguration.GetStandardHeaders();

        options.OperationFilter<StandardHeadersOperationFilter>();
    }

    /// <summary>
    /// Operation filter that adds standard headers to all operations.
    /// </summary>
    private class StandardHeadersOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
    {
        public void Apply(OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            var headers = SwaggerConfiguration.GetStandardHeaders();

            // Add X-API-Version header
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-API-Version",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new Microsoft.OpenApi.Any.OpenApiString("1.0")
                },
                Description = "API version (default: 1.0)"
            });

            // Add X-Language header
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Language",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<Microsoft.OpenApi.Any.IOpenApiAny>
                    {
                        new Microsoft.OpenApi.Any.OpenApiString("en"),
                        new Microsoft.OpenApi.Any.OpenApiString("zu"),
                        new Microsoft.OpenApi.Any.OpenApiString("st"),
                        new Microsoft.OpenApi.Any.OpenApiString("xh"),
                        new Microsoft.OpenApi.Any.OpenApiString("af")
                    },
                    Default = new Microsoft.OpenApi.Any.OpenApiString("en")
                },
                Description = "Language code: en (English), zu (Zulu), st (Sotho), xh (Xhosa), af (Afrikaans)"
            });

            // Add X-Correlation-ID header (automatically added by middleware, but documented here)
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Correlation-ID",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "uuid"
                },
                Description = "Correlation ID for distributed tracing (auto-generated if not provided)"
            });
        }
    }

    /// <summary>
    /// Helper extension to check if environment is staging.
    /// </summary>
    private static bool IsStaging(this IHostEnvironment environment)
    {
        return environment.EnvironmentName.Equals("Staging", StringComparison.OrdinalIgnoreCase);
    }
}
