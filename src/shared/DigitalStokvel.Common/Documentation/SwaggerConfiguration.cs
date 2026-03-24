using Microsoft.OpenApi.Models;

namespace DigitalStokvel.Common.Documentation;

/// <summary>
/// Centralized Swagger/OpenAPI configuration for Digital Stokvel services.
/// Provides consistent API documentation with versioning, authentication, and metadata.
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Service information for generating OpenAPI documentation.
    /// </summary>
    public class ServiceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = "v1";
        public string ContactName { get; set; } = "Digital Stokvel Team";
        public string ContactEmail { get; set; } = "support@stokvel.bank.co.za";
        public string TermsOfServiceUrl { get; set; } = "https://stokvel.bank.co.za/terms";
        public string LicenseName { get; set; } = "Proprietary";
    }

    /// <summary>
    /// Creates OpenAPI document information with service details.
    /// </summary>
    public static OpenApiInfo CreateOpenApiInfo(ServiceInfo serviceInfo)
    {
        return new OpenApiInfo
        {
            Title = $"Digital Stokvel - {serviceInfo.Name}",
            Version = serviceInfo.Version,
            Description = $@"
{serviceInfo.Description}

## Overview
The Digital Stokvel Banking platform brings South Africa's R50B informal savings economy into the formal banking system. 
This API supports Android, iOS, USSD, and Web platforms with POPIA, FICA, and SARB compliance built-in.

## Authentication
All endpoints require JWT Bearer token authentication unless marked as public.

```
Authorization: Bearer <jwt_token>
X-API-Version: 1.0
X-Language: en|zu|st|xh|af
```

## Rate Limiting
- 100 requests per minute per user
- 500 requests per minute per group

## Support
For technical support, contact {serviceInfo.ContactEmail}

## Compliance
- **POPIA** - Protection of Personal Information Act
- **FICA** - Financial Intelligence Centre Act
- **SARB** - South African Reserve Bank Regulations
",
            Contact = new OpenApiContact
            {
                Name = serviceInfo.ContactName,
                Email = serviceInfo.ContactEmail
            },
            TermsOfService = new Uri(serviceInfo.TermsOfServiceUrl),
            License = new OpenApiLicense
            {
                Name = serviceInfo.LicenseName
            }
        };
    }

    /// <summary>
    /// Configures JWT Bearer authentication for Swagger UI.
    /// </summary>
    public static OpenApiSecurityScheme CreateJwtSecurityScheme()
    {
        return new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header using the Bearer scheme.
                Enter your JWT token in the text input below.
                Example: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };
    }

    /// <summary>
    /// Creates the security requirement for JWT Bearer authentication.
    /// </summary>
    public static OpenApiSecurityRequirement CreateJwtSecurityRequirement()
    {
        return new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        };
    }

    /// <summary>
    /// Creates standard response examples for common HTTP status codes.
    /// </summary>
    public static Dictionary<int, string> GetStandardResponses()
    {
        return new Dictionary<int, string>
        {
            { 200, "Success - Request completed successfully" },
            { 201, "Created - Resource created successfully" },
            { 204, "No Content - Request completed, no content to return" },
            { 400, "Bad Request - Invalid request parameters" },
            { 401, "Unauthorized - Authentication required or invalid token" },
            { 403, "Forbidden - Insufficient permissions" },
            { 404, "Not Found - Resource not found" },
            { 409, "Conflict - Resource conflict (e.g., duplicate)" },
            { 422, "Unprocessable Entity - Validation failed" },
            { 429, "Too Many Requests - Rate limit exceeded" },
            { 500, "Internal Server Error - Unexpected server error" },
            { 503, "Service Unavailable - Service temporarily unavailable" }
        };
    }

    /// <summary>
    /// Gets standard API headers for documentation.
    /// </summary>
    public static List<(string Name, string Description, bool Required)> GetStandardHeaders()
    {
        return new List<(string Name, string Description, bool Required)>
        {
            ("Authorization", "JWT Bearer token for authentication", true),
            ("X-API-Version", "API version (default: 1.0)", false),
            ("X-Language", "Language code: en|zu|st|xh|af (default: en)", false),
            ("X-Correlation-ID", "Correlation ID for distributed tracing", false)
        };
    }

    /// <summary>
    /// Predefined service information for all Digital Stokvel services.
    /// </summary>
    public static class Services
    {
        public static ServiceInfo GroupService => new()
        {
            Name = "Group Service",
            Description = "Manages stokvel group creation, member management, role assignment, and group governance.",
            Version = "v1"
        };

        public static ServiceInfo ContributionService => new()
        {
            Name = "Contribution Service",
            Description = "Handles member contributions, payment processing, recurring payments, and contribution ledger (immutable).",
            Version = "v1"
        };

        public static ServiceInfo PayoutService => new()
        {
            Name = "Payout Service",
            Description = "Manages payout initiation, dual approval workflow (Chairperson + Treasurer), and disbursement processing.",
            Version = "v1"
        };

        public static ServiceInfo GovernanceService => new()
        {
            Name = "Governance Service",
            Description = "Handles voting, dispute resolution, missed payment detection, and late fee application.",
            Version = "v1"
        };

        public static ServiceInfo NotificationService => new()
        {
            Name = "Notification Service",
            Description = "Manages push notifications, SMS, and multilingual notification delivery (5 languages).",
            Version = "v1"
        };

        public static ServiceInfo CreditProfileService => new()
        {
            Name = "Credit Profile Service",
            Description = "Builds member credit profiles, calculates Stokvel Score, and provides pre-qualification for loans (P1 feature).",
            Version = "v1"
        };

        public static ServiceInfo UssdGateway => new()
        {
            Name = "USSD Gateway",
            Description = "Provides USSD session management for feature phone users (3-level menu, 120-second timeout).",
            Version = "v1"
        };

        public static ServiceInfo ApiGateway => new()
        {
            Name = "API Gateway",
            Description = "Single entry point for all client requests with routing, authentication, rate limiting, and protocol translation.",
            Version = "v1"
        };
    }
}
