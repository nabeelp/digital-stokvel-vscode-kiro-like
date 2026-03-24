# API Documentation Standards - Digital Stokvel Banking

**Version:** 1.0  
**Last Updated:** March 24, 2026  
**Status:** Active

---

## Overview

This document defines the standards and best practices for API documentation across all Digital Stokvel Banking microservices using Swagger/OpenAPI 3.0.

### Purpose

- Provide consistent, comprehensive API documentation across all services
- Enable interactive API testing through Swagger UI
- Support automated client code generation
- Facilitate developer onboarding and integration testing

### Technology Stack

- **Swagger/OpenAPI**: 3.0.x
- **Swashbuckle.AspNetCore**: 7.2.0
- **Authentication**: JWT Bearer tokens
- **API Versioning**: URL path versioning (e.g., `/v1/`)

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Service Configuration](#service-configuration)
3. [Swagger UI Access](#swagger-ui-access)
4. [Authentication in Swagger](#authentication-in-swagger)
5. [API Standards](#api-standards)
6. [Documentation Best Practices](#documentation-best-practices)
7. [XML Documentation](#xml-documentation)
8. [Common Schemas](#common-schemas)
9. [Testing with Swagger](#testing-with-swagger)
10. [Security Considerations](#security-considerations)
11. [Troubleshooting](#troubleshooting)

---

## Quick Start

### 1. Configure Swagger in Program.cs

```csharp
using DigitalStokvel.Common.Documentation;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger with Digital Stokvel configuration
builder.Services.AddDigitalStokvelSwagger(
    SwaggerConfiguration.Services.GroupService  // Use appropriate service
);

builder.Services.AddControllers();

var app = builder.Build();

// Enable Swagger UI (Development and Staging only)
app.UseDigitalStokvelSwaggerWithEnvironment(
    app.Environment,
    SwaggerConfiguration.Services.GroupService
);

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 2. Document Your Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DigitalStokvel.GroupService.Controllers;

/// <summary>
/// Manages stokvel group creation, member management, and governance.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Group management operations including creation, updates, and member invitations")]
public class GroupsController : ControllerBase
{
    /// <summary>
    /// Creates a new stokvel group.
    /// </summary>
    /// <param name="request">Group creation details</param>
    /// <returns>The created group with account information</returns>
    /// <response code="201">Group created successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Authentication required</response>
    /// <response code="422">Validation failed</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new stokvel group",
        Description = "Creates a new stokvel group with the specified configuration, " +
                      "creates a linked savings account, and sends member invitations.",
        OperationId = "CreateGroup",
        Tags = new[] { "Groups" }
    )]
    [SwaggerResponse(201, "Group created successfully", typeof(GroupResponse))]
    [SwaggerResponse(400, "Bad request", typeof(ErrorResponse))]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(422, "Validation failed", typeof(ValidationErrorResponse))]
    public async Task<ActionResult<GroupResponse>> CreateGroup(
        [FromBody, SwaggerRequestBody("Group creation details", Required = true)] 
        CreateGroupRequest request)
    {
        // Implementation
        return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
    }

    /// <summary>
    /// Gets a specific group by ID.
    /// </summary>
    /// <param name="id">The group ID (UUID)</param>
    /// <returns>Group details including balance and member count</returns>
    /// <response code="200">Group found</response>
    /// <response code="404">Group not found</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get group details",
        Description = "Retrieves detailed information about a specific stokvel group.",
        OperationId = "GetGroup"
    )]
    [SwaggerResponse(200, "Group found", typeof(GroupResponse))]
    [SwaggerResponse(404, "Group not found", typeof(ErrorResponse))]
    public async Task<ActionResult<GroupResponse>> GetGroup(
        [FromRoute, SwaggerParameter("Group unique identifier", Required = true)] 
        Guid id)
    {
        // Implementation
        return Ok(group);
    }
}
```

---

## Service Configuration

### Available Service Configurations

The `SwaggerConfiguration.Services` class provides predefined configurations for all services:

```csharp
// Group Service
SwaggerConfiguration.Services.GroupService

// Contribution Service
SwaggerConfiguration.Services.ContributionService

// Payout Service
SwaggerConfiguration.Services.PayoutService

// Governance Service
SwaggerConfiguration.Services.GovernanceService

// Notification Service
SwaggerConfiguration.Services.NotificationService

// Credit Profile Service
SwaggerConfiguration.Services.CreditProfileService

// USSD Gateway
SwaggerConfiguration.Services.UssdGateway

// API Gateway
SwaggerConfiguration.Services.ApiGateway
```

### Custom Service Configuration

If you need a custom configuration:

```csharp
var customServiceInfo = new SwaggerConfiguration.ServiceInfo
{
    Name = "Custom Service",
    Description = "Service description",
    Version = "v1",
    ContactName = "Team Name",
    ContactEmail = "team@stokvel.bank.co.za"
};

builder.Services.AddDigitalStokvelSwagger(customServiceInfo);
```

---

## Swagger UI Access

### URL Endpoints

Each service exposes Swagger documentation at:

```
Development: http://localhost:{port}/swagger
Staging: https://api-staging.stokvel.bank.co.za/{service}/swagger
```

**Example URLs:**
- Group Service: `http://localhost:5001/swagger`
- Contribution Service: `http://localhost:5002/swagger`
- Payout Service: `http://localhost:5003/swagger`

### OpenAPI JSON Endpoint

Raw OpenAPI JSON specification:

```
http://localhost:{port}/swagger/v1/swagger.json
```

Use this for:
- Client code generation tools (AutoRest, NSwag, OpenAPI Generator)
- API testing tools (Postman, Insomnia)
- API gateways (Kong, Azure APIM)

---

## Authentication in Swagger

### Obtaining a JWT Token

1. **Login via Authentication API** (when implemented):
   ```bash
   curl -X POST https://api.stokvel.bank.co.za/v1/auth/login \
     -H "Content-Type: application/json" \
     -d '{"phone_number": "+27821234567", "pin": "encrypted_pin"}'
   ```

2. **Copy the JWT token** from the response:
   ```json
   {
     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
     "expires_at": "2026-03-25T10:30:00Z"
   }
   ```

### Using JWT in Swagger UI

1. Click the **"Authorize"** button in Swagger UI (top-right, lock icon)
2. Paste the JWT token (without "Bearer" prefix)
3. Click **"Authorize"**
4. All subsequent requests will include the JWT token

**Token Format:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## API Standards

### URL Structure

```
https://{environment}.stokvel.bank.co.za/{version}/{resource}/{id}/{sub-resource}
```

**Examples:**
- `GET /v1/groups/{group_id}`
- `POST /v1/contributions`
- `GET /v1/groups/{group_id}/members`

### HTTP Methods

| Method   | Usage                  | Example                           |
|----------|------------------------|-----------------------------------|
| `GET`    | Retrieve resource(s)   | `GET /v1/groups/{id}`             |
| `POST`   | Create resource        | `POST /v1/groups`                 |
| `PUT`    | Full update            | `PUT /v1/groups/{id}`             |
| `PATCH`  | Partial update         | `PATCH /v1/groups/{id}/status`    |
| `DELETE` | Delete resource        | `DELETE /v1/groups/{id}`          |

### Standard Headers

All API requests support these headers:

```http
Authorization: Bearer <jwt_token>          # Required (except public endpoints)
X-API-Version: 1.0                         # Optional (default: 1.0)
X-Language: en|zu|st|xh|af                 # Optional (default: en)
X-Correlation-ID: <uuid>                   # Optional (auto-generated if missing)
Content-Type: application/json             # Required for POST/PUT/PATCH
```

### Response Status Codes

| Code | Meaning                     | Usage                                      |
|------|-----------------------------|-------------------------------------------|
| 200  | OK                          | Successful GET, PUT, PATCH                |
| 201  | Created                     | Successful POST                           |
| 204  | No Content                  | Successful DELETE                         |
| 400  | Bad Request                 | Invalid request syntax                    |
| 401  | Unauthorized                | Missing or invalid JWT token              |
| 403  | Forbidden                   | Insufficient permissions                  |
| 404  | Not Found                   | Resource doesn't exist                    |
| 409  | Conflict                    | Resource already exists                   |
| 422  | Unprocessable Entity        | Validation failed                         |
| 429  | Too Many Requests           | Rate limit exceeded                       |
| 500  | Internal Server Error       | Unexpected server error                   |
| 503  | Service Unavailable         | Service temporarily unavailable           |

### Standard Response Schemas

#### Success Response

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Ntombizodwa Savings",
  "status": "active",
  "created_at": "2026-03-24T10:30:00Z"
}
```

#### Error Response

```json
{
  "error": {
    "code": "RESOURCE_NOT_FOUND",
    "message": "Group with ID 12345 not found",
    "correlation_id": "abc-123-def",
    "timestamp": "2026-03-24T14:30:00Z"
  }
}
```

#### Validation Error Response

```json
{
  "error": {
    "code": "VALIDATION_FAILED",
    "message": "One or more validation errors occurred",
    "correlation_id": "abc-123-def",
    "timestamp": "2026-03-24T14:30:00Z",
    "validation_errors": [
      {
        "field": "contribution_amount",
        "message": "Must be greater than 0"
      },
      {
        "field": "name",
        "message": "Name is required"
      }
    ]
  }
}
```

---

## Documentation Best Practices

### Controller Documentation

```csharp
/// <summary>
/// Brief description of the controller's purpose (1-2 sentences).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Detailed description of operations this controller handles")]
public class ExampleController : ControllerBase
{
    // ...
}
```

### Endpoint Documentation

```csharp
/// <summary>
/// Brief description of what the endpoint does (1 sentence).
/// </summary>
/// <param name="id">Description of the parameter</param>
/// <returns>Description of what is returned</returns>
/// <response code="200">Success case description</response>
/// <response code="404">Error case description</response>
/// <remarks>
/// Detailed explanation, examples, business rules, etc.
/// 
/// Example request:
/// 
///     POST /api/v1/groups
///     {
///         "name": "Ntombizodwa Savings",
///         "contribution_amount": 500.00
///     }
/// 
/// </remarks>
[HttpGet("{id}")]
[SwaggerOperation(
    Summary = "Short summary",
    Description = "Detailed description with context",
    OperationId = "UniqueOperationId"
)]
[SwaggerResponse(200, "Success", typeof(ResponseType))]
[SwaggerResponse(404, "Not found", typeof(ErrorResponse))]
public async Task<ActionResult<ResponseType>> GetItem(Guid id)
{
    // Implementation
}
```

### Model Documentation

```csharp
/// <summary>
/// Represents a stokvel group in the Digital Stokvel system.
/// </summary>
public class GroupResponse
{
    /// <summary>
    /// Unique identifier for the group (UUID).
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    [SwaggerSchema("Group unique identifier")]
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the stokvel group (3-50 characters).
    /// </summary>
    /// <example>Ntombizodwa Savings</example>
    [SwaggerSchema("Group name", MinLength = 3, MaxLength = 50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Monthly contribution amount in ZAR.
    /// </summary>
    /// <example>500.00</example>
    [SwaggerSchema("Contribution amount", Format = "decimal", Minimum = 0)]
    public decimal ContributionAmount { get; set; }

    /// <summary>
    /// Group status: active, inactive, suspended.
    /// </summary>
    /// <example>active</example>
    [SwaggerSchema("Group status")]
    public GroupStatus Status { get; set; }
}
```

### Enum Documentation

```csharp
/// <summary>
/// Represents the status of a stokvel group.
/// </summary>
public enum GroupStatus
{
    /// <summary>
    /// Group is active and accepting contributions.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Group is inactive but can be reactivated.
    /// </summary>
    Inactive = 1,

    /// <summary>
    /// Group is suspended due to policy violation.
    /// </summary>
    Suspended = 2
}
```

---

## XML Documentation

### Enable XML Documentation

XML documentation is **already enabled** in `Directory.Build.props`:

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn> <!-- Suppress missing XML comment warnings -->
</PropertyGroup>
```

### XML Documentation Guidelines

1. **All public APIs must have XML comments**
2. Use `<summary>` for brief descriptions (1-2 sentences)
3. Use `<remarks>` for detailed explanations and examples
4. Document all parameters with `<param>`
5. Document return values with `<returns>`
6. Document exceptions with `<exception>` (if applicable)
7. Provide `<example>` tags with realistic data

---

## Common Schemas

### Pagination

```csharp
/// <summary>
/// Pagination information for list responses.
/// </summary>
public class PaginationInfo
{
    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int Pages { get; set; }
}

/// <summary>
/// Paginated list response.
/// </summary>
public class PaginatedResponse<T>
{
    /// <summary>
    /// List of items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Pagination metadata.
    /// </summary>
    public PaginationInfo Pagination { get; set; } = new();
}
```

### Error Response

```csharp
/// <summary>
/// Standard error response structure.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error details.
    /// </summary>
    public ErrorDetails Error { get; set; } = new();
}

/// <summary>
/// Error details.
/// </summary>
public class ErrorDetails
{
    /// <summary>
    /// Machine-readable error code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Correlation ID for distributed tracing.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the error occurred (ISO 8601).
    /// </summary>
    public DateTime Timestamp { get; set; }
}
```

---

## Testing with Swagger

### Interactive Testing

1. Open Swagger UI: `http://localhost:{port}/swagger`
2. Click **"Authorize"** and add your JWT token
3. Expand an endpoint
4. Click **"Try it out"**
5. Fill in parameters/body
6. Click **"Execute"**
7. View response

### cURL Export

Swagger UI generates cURL commands for each request:

```bash
curl -X POST "http://localhost:5001/api/v1/groups" \
  -H "accept: application/json" \
  -H "Authorization: Bearer eyJhbG..." \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Ntombizodwa Savings",
    "contribution_amount": 500.00
  }'
```

### Client Code Generation

Generate client SDKs using NSwag or OpenAPI Generator:

```bash
# Generate C# client
nswag openapi2csclient /input:http://localhost:5001/swagger/v1/swagger.json \
  /output:DigitalStokvel.Client.cs \
  /namespace:DigitalStokvel.Client

# Generate TypeScript client
openapi-generator-cli generate \
  -i http://localhost:5001/swagger/v1/swagger.json \
  -g typescript-axios \
  -o ./clients/typescript
```

---

## Security Considerations

### Production Access

**⚠️ Swagger UI is DISABLED in Production by default** for security reasons.

To enable Swagger in production (not recommended):

```csharp
app.UseDigitalStokvelSwaggerWithEnvironment(
    app.Environment,
    serviceInfo,
    enableInProduction: true  // NOT RECOMMENDED
);
```

### Sensitive Data

**NEVER expose sensitive data in Swagger documentation:**

- ❌ Real passwords, PINs, or tokens in examples
- ❌ Personal identification numbers (ID numbers)
- ❌ Banking account numbers (except test data)
- ❌ Credit card details

**Use realistic but fake test data:**

```csharp
/// <example>+27821234567</example>  ✓ Good (fake phone number)
/// <example>+27829876543</example>  ✓ Good (fake phone number)
/// <example>1234</example>           ✓ Good (test PIN)
```

### API Key Storage

If you must expose Swagger in production:

1. Protect with API key authentication
2. Use IP whitelisting
3. Monitor access logs
4. Disable verbose error messages

---

## Troubleshooting

### Swagger UI Not Loading

**Issue:** Swagger UI shows blank page or 404.

**Solutions:**
1. Verify `app.UseSwagger()` is called before `app.UseSwaggerUI()`
2. Check `app.Environment.IsDevelopment()` condition
3. Ensure `builder.Services.AddSwaggerGen()` is called
4. Verify route prefix: `/swagger`

### JWT Authentication Not Working

**Issue:** "Authorization has been denied" error.

**Solutions:**
1. Verify JWT token is valid (not expired)
2. Check token is entered without "Bearer" prefix in Swagger UI
3. Ensure `app.UseAuthorization()` is called
4. Verify JWT middleware is configured

### XML Comments Not Showing

**Issue:** API documentation is missing descriptions.

**Solutions:**
1. Ensure `GenerateDocumentationFile` is enabled in `.csproj`
2. Rebuild the project
3. Verify `options.IncludeXmlComments()` is called in Swagger configuration
4. Check XML file exists in `bin/Debug/net10.0/` folder

### Schema Conflicts

**Issue:** "SchemaId already added" error.

**Solutions:**
1. Use `options.CustomSchemaIds(type => type.FullName)` in Swagger configuration
2. Ensure classes with same name have different namespaces
3. Use `[SwaggerSchema]` attribute to customize schema ID

---

## Example: Complete Controller Implementation

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DigitalStokvel.GroupService.Controllers;

/// <summary>
/// Manages stokvel groups
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
[SwaggerTag("Group management - create, update, list groups")]
public class GroupsController : ControllerBase
{
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(ILogger<GroupsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a new stokvel group
    /// </summary>
    /// <param name="request">Group creation details</param>
    /// <returns>Created group with account information</returns>
    /// <response code="201">Group created successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="422">Validation failed</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new stokvel group",
        Description = "Creates a new stokvel group with linked savings account",
        OperationId = "CreateGroup",
        Tags = new[] { "Groups" }
    )]
    [SwaggerResponse(201, "Group created", typeof(GroupResponse))]
    [SwaggerResponse(400, "Bad request", typeof(ErrorResponse))]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(422, "Validation failed", typeof(ValidationErrorResponse))]
    public async Task<ActionResult<GroupResponse>> CreateGroup(
        [FromBody, SwaggerRequestBody("Group details", Required = true)] 
        CreateGroupRequest request)
    {
        _logger.LogInformation("Creating group: {GroupName}", request.Name);

        // Simulate creation
        var group = new GroupResponse
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ContributionAmount = request.ContributionAmount,
            Status = GroupStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
    }

    /// <summary>
    /// Gets a specific group by ID
    /// </summary>
    /// <param name="id">Group unique identifier</param>
    /// <returns>Group details</returns>
    /// <response code="200">Group found</response>
    /// <response code="404">Group not found</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get group details",
        Description = "Retrieves detailed information about a stokvel group",
        OperationId = "GetGroup"
    )]
    [SwaggerResponse(200, "Group found", typeof(GroupResponse))]
    [SwaggerResponse(404, "Not found", typeof(ErrorResponse))]
    public async Task<ActionResult<GroupResponse>> GetGroup(
        [FromRoute, SwaggerParameter("Group ID", Required = true)] 
        Guid id)
    {
        _logger.LogInformation("Fetching group: {GroupId}", id);
        
        // Simulate retrieval
        var group = new GroupResponse
        {
            Id = id,
            Name = "Ntombizodwa Savings",
            ContributionAmount = 500.00m,
            Status = GroupStatus.Active,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        return Ok(group);
    }
}

/// <summary>
/// Group creation request
/// </summary>
public class CreateGroupRequest
{
    /// <summary>
    /// Group name (3-50 characters)
    /// </summary>
    /// <example>Ntombizodwa Savings</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Monthly contribution amount in ZAR
    /// </summary>
    /// <example>500.00</example>
    public decimal ContributionAmount { get; set; }
}

/// <summary>
/// Group response
/// </summary>
public class GroupResponse
{
    /// <summary>
    /// Group ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Group name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Contribution amount
    /// </summary>
    public decimal ContributionAmount { get; set; }

    /// <summary>
    /// Group status
    /// </summary>
    public GroupStatus Status { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Group status
/// </summary>
public enum GroupStatus
{
    /// <summary>
    /// Active group
    /// </summary>
    Active = 0,

    /// <summary>
    /// Inactive group
    /// </summary>
    Inactive = 1,

    /// <summary>
    /// Suspended group
    /// </summary>
    Suspended = 2
}
```

---

## Checklist for API Documentation

Before marking an API complete, verify:

- [ ] All controllers have XML summary comments
- [ ] All endpoints have XML documentation (`<summary>`, `<param>`, `<returns>`)
- [ ] All response codes are documented with `<response>` tags
- [ ] All models have XML comments on properties
- [ ] `[SwaggerOperation]` attributes are used for detailed descriptions
- [ ] `[SwaggerResponse]` attributes specify response types
- [ ] Examples are provided using `<example>` tags
- [ ] Swagger UI loads without errors
- [ ] Authentication works in Swagger UI
- [ ] All endpoints can be tested interactively
- [ ] OpenAPI JSON validates (https://editor.swagger.io/)

---

## References

- [Swashbuckle Documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [OpenAPI Specification 3.0](https://swagger.io/specification/)
- [Microsoft API Documentation Guidelines](https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/swagger)
- [Digital Stokvel Technical Design](./design.md)
- [Digital Stokvel Coding Standards](./CODING_STANDARDS.md)

---

**Document Status:** Active  
**Last Updated:** March 24, 2026  
**Next Review:** April 2026
