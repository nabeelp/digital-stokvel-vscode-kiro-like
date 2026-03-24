# Digital Stokvel Banking - Logging Standards

**Version:** 1.0  
**Last Updated:** March 2026

---

## Overview

This document defines the logging standards for the Digital Stokvel Banking platform. All services use **Serilog** for structured logging with consistent configuration across the platform.

## Logging Framework

**Primary Logger:** Serilog 4.x  
**Structured Logging:** Yes  
**Distributed Tracing:** Correlation IDs  
**Log Aggregation:** Seq (optional), ELK Stack (production)

---

## Configuration

### Development Environment

```csharp
using DigitalStokvel.Common.Logging;
using Serilog;

// In Program.cs
Log.Logger = LoggingConfiguration
    .ConfigureForDevelopment("GroupService")
    .CreateLogger();
```

### Production Environment

```csharp
// In Program.cs
var configuration = builder.Configuration;

Log.Logger = LoggingConfiguration
    .ConfigureForProduction(
        serviceName: "GroupService",
        configuration: configuration,
        seqServerUrl: configuration["Serilog:SeqServerUrl"],
        seqApiKey: configuration["Serilog:SeqApiKey"])
    .CreateLogger();
```

### ASP.NET Core Integration

```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

var app = builder.Build();

// Add correlation ID middleware
app.UseMiddleware<CorrelationIdMiddleware>();

app.Run();
```

---

## Log Levels

Use log levels consistently across all services:

| Level | Usage | Examples |
|-------|-------|----------|
| **Trace** | Very detailed diagnostic information | Query execution plans, cache internals |
| **Debug** | Internal state, development diagnostics | Cache hits/misses, validation steps |
| **Information** | General application flow | User actions, business events |
| **Warning** | Unexpected but recoverable situations | Retries, fallbacks, capacity warnings |
| **Error** | Errors that stop an operation | Payment failures, validation errors |
| **Critical** | System-wide failures | Database unavailable, critical service down |

---

## Structured Logging Patterns

### Basic Logging

```csharp
_logger.LogInformation("Group {GroupId} created by user {UserId}", groupId, userId);
```

### With Context Scopes

```csharp
using (_logger.BeginCorrelationScope(correlationId))
using (_logger.BeginGroupScope(groupId, groupName))
{
    _logger.LogInformation("Processing contribution for group");
    // Correlation ID and Group ID automatically included
}
```

### User Context

```csharp
using (_logger.BeginUserScope(userId, userName))
{
    _logger.LogInformation("User performed action");
}
```

### Transaction Context

```csharp
using (_logger.BeginTransactionScope("Contribution", contributionId, amount))
{
    _logger.LogInformation("Processing payment");
}
```

### Performance Tracking

```csharp
using (_logger.BeginPerformanceScope("ProcessContribution", $"Group: {groupId}"))
{
    // Operation code
    // Automatically logs duration on dispose
}
```

### Sensitive Data

```csharp
_logger.LogSensitive(
    LogLevel.Information,
    "User login",
    ("PhoneNumber", phoneNumber),
    ("IdNumber", idNumber)
);
// Output: User login PhoneNumber=****1234, IdNumber=****5678
```

---

## Standard Log Messages

Use the predefined `LogMessages` class for consistency:

### Group Service

```csharp
LogMessages.GroupService.GroupCreated(_logger, groupId, groupName, createdBy);
LogMessages.GroupService.MemberAdded(_logger, groupId, userId, role);
LogMessages.GroupService.GroupAtCapacity(_logger, groupId, current, max);
```

### Contribution Service

```csharp
LogMessages.ContributionService.ContributionReceived(_logger, id, groupId, memberId, amount);
LogMessages.ContributionService.PaymentProcessing(_logger, id, transactionRef);
LogMessages.ContributionService.PaymentFailed(_logger, id, reason);
```

### Payout Service

```csharp
LogMessages.PayoutService.PayoutInitiated(_logger, id, groupId, amount, initiatedBy);
LogMessages.PayoutService.PayoutApproved(_logger, id, approvedBy);
LogMessages.PayoutService.DualApprovalComplete(_logger, id, initiator, approver);
```

### Security

```csharp
LogMessages.Security.AuthenticationSuccessful(_logger, userId, phoneNumber);
LogMessages.Security.AuthenticationFailed(_logger, phoneNumber, reason);
LogMessages.Security.UnauthorizedAccess(_logger, userId, resource, action);
```

---

## Correlation IDs

Every HTTP request automatically gets a correlation ID for distributed tracing.

### Automatic (Middleware)

```csharp
app.UseMiddleware<CorrelationIdMiddleware>();
```

### Manual

```csharp
var correlationId = Guid.NewGuid().ToString();
using (_logger.BeginCorrelationScope(correlationId))
{
    // All logs include CorrelationId
}
```

### Passing Between Services

```csharp
var request = new HttpRequestMessage(HttpMethod.Get, url);
request.Headers.Add("X-Correlation-ID", correlationId);
```

---

## Log Sinks Configuration

### Console (Development)

- **Purpose:** Real-time debugging
- **Format:** Human-readable
- **Level:** Debug

### File (All Environments)

- **Path:** `logs/digitalstokvel-{Date}.log`
- **Rolling:** Daily
- **Size Limit:** 100MB per file
- **Retention:** 30 days
- **Level:** Information (Production), Debug (Development)

### Seq (Optional - Development/Staging)

- **Purpose:** Centralized log aggregation and querying
- **URL:** Configure in `appsettings.json`
- **Level:** Debug
- **Features:** Real-time filtering, structured queries, dashboards

### ELK Stack (Production)

- **Purpose:** Production log aggregation
- **Components:** Elasticsearch, Logstash, Kibana
- **Level:** Information
- **Integration:** Via Filebeat or direct Elasticsearch sink

---

## Configuration File

### appsettings.Development.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "restrictedToMinimumLevel": "Debug"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/dev/digitalstokvel-.log",
          "rollingInterval": "Day",
          "restrictedToMinimumLevel": "Debug"
        }
      }
    ]
  }
}
```

### appsettings.Production.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "restrictedToMinimumLevel": "Information"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/digitalstokvel-.log",
          "rollingInterval": "Day",
          "restrictedToMinimumLevel": "Warning",
          "retainedFileCountLimit": 30
        }
      }
    ],
    "SeqServerUrl": "https://seq.digitalstokvel.local",
    "SeqApiKey": "<api-key-from-key-vault>"
  }
}
```

---

## Best Practices

### Do's ✅

- Use structured logging with named properties
- Include correlation IDs for all requests
- Log business events (group created, payment received)
- Use appropriate log levels
- Mask sensitive data (phone numbers, ID numbers)
- Log performance metrics for slow operations
- Include context (user ID, group ID, transaction ID)

### Don'ts ❌

- Don't log passwords or PINs
- Don't log full credit card numbers or bank details
- Don't use string interpolation in log messages
- Don't log excessive data in production
- Don't use `Console.WriteLine` or `Debug.WriteLine`
- Don't log inside tight loops without thresholding

---

## Security Considerations

### POPIA Compliance

- Mask all PII in logs (phone numbers, ID numbers)
- Never log unencrypted sensitive data
- Set appropriate log retention (7 years for financial transactions)
- Implement access controls on log files

### Sensitive Data Handling

```csharp
// Bad ❌
_logger.LogInformation("User {IdNumber} logged in", idNumber);

// Good ✅
_logger.LogSensitive(LogLevel.Information, "User logged in", ("IdNumber", idNumber));
// Output: User logged in IdNumber=****5678
```

---

## Monitoring & Alerting

### Key Metrics to Monitor

- Error rate (errors per minute)
- Warning rate (warnings per minute)
- Response time (p50, p95, p99)
- Failed authentication attempts
- Payment failures
- External service failures

### Alert Rules

- **Critical:** > 10 errors/minute
- **Warning:** > 5 authentication failures from same IP
- **Warning:** Payment gateway failure rate > 5%
- **Info:** Slow query (> 1 second)

---

## Log Analysis Queries

### Seq Queries

```sql
-- Find all errors for a specific user
UserId = '123e4567-e89b-12d3-a456-426614174000' AND @Level = 'Error'

-- Find slow operations
@mt = 'Completed operation: {OperationName} in {Duration}ms' AND Duration > 1000

-- Find failed payments
@mt LIKE '%Payment failed%'

-- Trace a specific request
CorrelationId = 'abc-123-def'
```

---

## Testing Logging

### Unit Tests

```csharp
using Microsoft.Extensions.Logging;
using Moq;

[Fact]
public void Should_Log_Group_Creation()
{
    // Arrange
    var mockLogger = new Mock<ILogger<GroupService>>();
    var service = new GroupService(mockLogger.Object);

    // Act
    service.CreateGroup(groupRequest);

    // Assert
    mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Group created")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
}
```

---

## Troubleshooting

### No Logs Appearing

1. Check Serilog is configured in `Program.cs`
2. Verify `UseSerilog()` is called
3. Check minimum log level in `appsettings.json`
4. Ensure logger is injected via DI

### Logs Missing Context

1. Verify correlation ID middleware is registered
2. Check log context scopes are properly disposed
3. Ensure `Enrich.FromLogContext()` is configured

### Performance Issues

1. Use async sinks for file/remote logging
2. Reduce log level in production
3. Implement sampling for high-volume operations
4. Use structured logging (avoid string concatenation)

---

## Resources

- [Serilog Documentation](https://serilog.net/)
- [Seq Documentation](https://docs.datalust.co/docs)
- [ELK Stack Guide](https://www.elastic.co/guide/)
- [Logging Best Practices](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/)

---

**Next Steps:**
1. Review this document with the team
2. Implement logging in each microservice
3. Set up Seq for development/staging
4. Configure ELK Stack for production
5. Create monitoring dashboards
