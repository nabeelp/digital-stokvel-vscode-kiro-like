# Testing Standards - Digital Stokvel Banking

**Version:** 1.0  
**Last Updated:** March 24, 2026  
**Status:** Active

---

## Overview

This document defines the testing standards, strategies, and guidelines for the Digital Stokvel Banking platform. All developers must follow these standards to ensure code quality, reliability, and maintainability.

---

## Testing Philosophy

### Core Principles

1. **Test-Driven Development (TDD):** Write tests before implementation where feasible
2. **Test Pyramid:** Emphasize unit tests, followed by integration tests, then end-to-end tests
3. **Coverage Goals:** Minimum 80% code coverage for all services
4. **Fast Feedback:** Tests should run quickly to support rapid iteration
5. **Isolation:** Unit tests should not depend on external services or databases

### Test Types

| Test Type | Purpose | Coverage | Speed | Tools |
|-----------|---------|----------|-------|-------|
| **Unit Tests** | Test individual components in isolation | 80%+ | Very Fast (<1s per test) | xUnit, Moq, FluentAssertions |
| **Integration Tests** | Test interactions between components | Key flows | Fast (1-5s per test) | xUnit, TestContainers, WebApplicationFactory |
| **End-to-End Tests** | Test complete user journeys | Critical paths | Slow (>5s per test) | Playwright, Selenium |
| **Load Tests** | Test performance under load | Scalability targets | Very Slow (minutes) | k6, JMeter, NBomber |

---

## Unit Testing Standards

### Project Structure

Unit tests are organized by service under `tests/unit/`:

```
tests/
└── unit/
    ├── DigitalStokvel.GroupService.Tests/
    ├── DigitalStokvel.ContributionService.Tests/
    ├── DigitalStokvel.PayoutService.Tests/
    ├── DigitalStokvel.GovernanceService.Tests/
    └── DigitalStokvel.NotificationService.Tests/
```

### Test Project Configuration

Each test project uses:
- **Framework:** xUnit 2.9+
- **Mocking:** Moq 4.20+
- **Assertions:** FluentAssertions 7.0+
- **Coverage:** coverlet.collector 6.0+

### Naming Conventions

**Test Classes:**
```csharp
[ClassName]Tests.cs
// Example: GroupServiceTests.cs, GroupRepositoryTests.cs
```

**Test Methods:**
```csharp
[MethodName]_[Scenario]_[ExpectedResult]()

// Examples:
CreateGroup_WithValidData_ShouldReturnGroupId()
GetGroup_WhenGroupNotFound_ShouldThrowNotFoundException()
AddMember_WithInvalidRole_ShouldReturnValidationError()
```

### Test Structure (AAA Pattern)

```csharp
[Fact]
public void CreateGroup_WithValidData_ShouldReturnGroupId()
{
    // Arrange - Set up test data and mocks
    var groupName = "Mandela Stokvel";
    var contributionAmount = 500m;
    var mockRepository = new Mock<IGroupRepository>();
    mockRepository.Setup(r => r.Create(It.IsAny<Group>()))
        .ReturnsAsync(Guid.NewGuid());
    
    var service = new GroupService(mockRepository.Object);
    
    // Act - Execute the operation being tested
    var result = await service.CreateGroupAsync(groupName, contributionAmount);
    
    // Assert - Verify the outcome
    result.Should().NotBeEmpty();
    mockRepository.Verify(r => r.Create(It.IsAny<Group>()), Times.Once);
}
```

### Test Categories

Use xUnit traits to categorize tests:

```csharp
[Trait("Category", "Unit")]
[Trait("Component", "GroupService")]
public class GroupServiceTests { }

[Trait("Category", "Integration")]
[Trait("Component", "Database")]
public class GroupRepositoryTests { }
```

### Code Coverage Requirements

| Component | Minimum Coverage | Target Coverage |
|-----------|------------------|-----------------|
| **Services** | 80% | 90% |
| **Repositories** | 75% | 85% |
| **Domain Models** | 70% | 80% |
| **Controllers** | 70% | 80% |
| **Utilities** | 80% | 95% |

**Exclusions from coverage:**
- Auto-generated code
- DTOs and simple POCOs
- Configuration classes
- Program.cs / Startup.cs boilerplate

---

## Integration Testing Standards

### Test Containers

Use Testcontainers.NET for integration tests requiring external dependencies:

```csharp
public class GroupRepositoryIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgresContainer;
    private GroupRepository _repository;
    
    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("digitalstokvel_test")
            .Build();
        
        await _postgresContainer.StartAsync();
        
        _repository = new GroupRepository(
            _postgresContainer.GetConnectionString()
        );
    }
    
    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
```

### WebApplicationFactory Tests

For API integration tests:

```csharp
public class GroupApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public GroupApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateGroup_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateGroupRequest
        {
            Name = "Test Stokvel",
            ContributionAmount = 500
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/groups", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

---

## Mocking Guidelines

### When to Mock

✅ **Mock these:**
- External services (APIs, databases, message queues)
- Time-dependent operations (`IDateTimeProvider`)
- File system operations
- Third-party libraries with network calls

❌ **Don't mock these:**
- Domain models
- Value objects
- Simple utilities
- Your own code in the same layer

### Moq Best Practices

```csharp
// ✅ GOOD - Specific setup with verification
var mockRepo = new Mock<IGroupRepository>();
mockRepo.Setup(r => r.GetByIdAsync(groupId))
    .ReturnsAsync(expectedGroup);

var result = await service.GetGroupAsync(groupId);

mockRepo.Verify(r => r.GetByIdAsync(groupId), Times.Once);

// ❌ BAD - Over-mocking, tight coupling to implementation
var mockRepo = new Mock<IGroupRepository>(MockBehavior.Strict);
mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
    .Returns(Task.FromResult<Group>(null));
```

### FluentAssertions Examples

```csharp
// Object comparison
result.Should().BeEquivalentTo(expected);

// Collections
groups.Should().HaveCount(5);
groups.Should().Contain(g => g.Name == "Mandela Stokvel");
groups.Should().AllSatisfy(g => g.ContributionAmount.Should().BeGreaterThan(0));

// Exceptions
Func<Task> act = async () => await service.DeleteGroupAsync(invalidId);
await act.Should().ThrowAsync<NotFoundException>()
    .WithMessage("*not found*");

// Dates
createdAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
```

---

## CI/CD Integration

### GitHub Actions Workflows

**CI Build Workflow (`.github/workflows/ci-build.yml`):**
- Runs on every push and pull request
- Executes all unit tests in Debug and Release configurations
- Collects code coverage with coverlet
- Uploads coverage to Codecov

**PR Validation Workflow (`.github/workflows/pr-validation.yml`):**
- Runs unit tests with coverage
- Generates coverage report with ReportGenerator
- Posts coverage summary to PR comments
- Fails if coverage drops below threshold

### Running Tests Locally

**Run all tests:**
```bash
dotnet test
```

**Run with coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

**Run specific test project:**
```bash
dotnet test tests/unit/DigitalStokvel.GroupService.Tests/DigitalStokvel.GroupService.Tests.csproj
```

**Run tests matching pattern:**
```bash
dotnet test --filter "FullyQualifiedName~GroupService"
```

**Run with detailed output:**
```bash
dotnet test --verbosity normal
```

### Coverage Report Generation

**Generate HTML report locally:**
```bash
# Install ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Generate report
reportgenerator \
  -reports:"./coverage/**/coverage.cobertura.xml" \
  -targetdir:"./coverage-report" \
  -reporttypes:Html

# Open report
start ./coverage-report/index.html  # Windows
open ./coverage-report/index.html   # macOS
```

---

## Test Data Management

### Test Data Builders

Use the Builder pattern for complex test data:

```csharp
public class GroupBuilder
{
    private string _name = "Default Stokvel";
    private decimal _contributionAmount = 500m;
    private List<GroupMember> _members = new();
    
    public GroupBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public GroupBuilder WithContributionAmount(decimal amount)
    {
        _contributionAmount = amount;
        return this;
    }
    
    public GroupBuilder WithMember(GroupMember member)
    {
        _members.Add(member);
        return this;
    }
    
    public Group Build()
    {
        return new Group
        {
            Id = Guid.NewGuid(),
            Name = _name,
            ContributionAmount = _contributionAmount,
            Members = _members,
            CreatedAt = DateTime.UtcNow
        };
    }
}

// Usage
var group = new GroupBuilder()
    .WithName("Mandela Stokvel")
    .WithContributionAmount(1000m)
    .Build();
```

### Fixture Data

Use xUnit class fixtures for shared test data:

```csharp
public class GroupTestFixture : IDisposable
{
    public List<Group> SampleGroups { get; }
    
    public GroupTestFixture()
    {
        SampleGroups = new List<Group>
        {
            new GroupBuilder().WithName("Group 1").Build(),
            new GroupBuilder().WithName("Group 2").Build(),
            new GroupBuilder().WithName("Group 3").Build()
        };
    }
    
    public void Dispose()
    {
        // Cleanup if needed
    }
}

public class GroupServiceTests : IClassFixture<GroupTestFixture>
{
    private readonly GroupTestFixture _fixture;
    
    public GroupServiceTests(GroupTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public void Test_UsingFixtureData()
    {
        var groups = _fixture.SampleGroups;
        // Use fixture data in test
    }
}
```

---

## Performance Testing

### Load Testing with k6

Example k6 script for API load testing:

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '30s', target: 100 },  // Ramp up to 100 users
    { duration: '1m', target: 100 },   // Stay at 100 users
    { duration: '30s', target: 0 },    // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000'], // 95% of requests under 2s
    http_req_failed: ['rate<0.01'],    // Less than 1% error rate
  },
};

export default function () {
  const res = http.get('https://api-staging.stokvel.bank.co.za/api/groups');
  
  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time OK': (r) => r.timings.duration < 2000,
  });
  
  sleep(1);
}
```

---

## Best Practices

### DO ✅

1. **Write tests first** (TDD) or immediately after implementation
2. **Keep tests simple** and focused on one behavior
3. **Use descriptive test names** that explain the scenario
4. **Test edge cases** and error conditions
5. **Mock external dependencies** to isolate units
6. **Use test fixtures** for shared setup
7. **Assert one concept** per test
8. **Clean up after tests** (IDisposable, IAsyncLifetime)
9. **Run tests before committing** code
10. **Maintain test quality** as production code

### DON'T ❌

1. **Don't test framework code** (e.g., EF Core, ASP.NET Core)
2. **Don't write brittle tests** that break with small refactoring
3. **Don't ignore flaky tests** - fix them immediately
4. **Don't test implementation details** - test behavior
5. **Don't use [Fact] for parameterized tests** - use [Theory]
6. **Don't share state** between tests
7. **Don't use Thread.Sleep** - use proper async/await
8. **Don't over-mock** - only mock external dependencies
9. **Don't skip tests** without a good reason (use [Skip] sparingly)
10. **Don't copy-paste tests** - use theories or fixtures

---

## Test Maintenance

### Refactoring Tests

When refactoring tests:
1. Keep existing tests passing
2. Add new tests for new behavior
3. Remove obsolete tests
4. Update test names to reflect new behavior
5. Consolidate duplicate tests

### Handling Flaky Tests

If a test is flaky:
1. **Investigate immediately** - don't ignore
2. **Check for race conditions** in async code
3. **Look for timing dependencies** (Thread.Sleep)
4. **Verify test isolation** - no shared state
5. **Fix or quarantine** (mark as [Skip] temporarily)

### Code Review Checklist

For test code reviews, verify:
- [ ] Tests follow AAA pattern
- [ ] Test names are descriptive
- [ ] Tests are isolated and don't depend on order
- [ ] Mocks are properly verified
- [ ] Edge cases are covered
- [ ] Tests are not overly complex
- [ ] Test data is clear and minimal
- [ ] Coverage meets minimum threshold

---

## Tools and Resources

### Required Tools

| Tool | Purpose | Installation |
|------|---------|--------------|
| **.NET 10 SDK** | Runtime for tests | [Download](https://dotnet.microsoft.com/download) |
| **xUnit** | Test framework | `dotnet add package xunit` |
| **Moq** | Mocking framework | `dotnet add package Moq` |
| **FluentAssertions** | Assertion library | `dotnet add package FluentAssertions` |
| **coverlet.collector** | Code coverage | `dotnet add package coverlet.collector` |

### Recommended Tools

- **ReportGenerator:** Coverage report visualization
- **Testcontainers:** Docker containers for integration tests
- **Bogus:** Fake data generation
- **AutoFixture:** Test data generation
- **Respawn:** Database cleanup for integration tests

### Learning Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Microsoft Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [Test-Driven Development (TDD) Guide](https://martinfowler.com/bliki/TestDrivenDevelopment.html)

---

## Appendix: Example Test Suite

### GroupServiceTests.cs (Complete Example)

```csharp
using DigitalStokvel.GroupService.Models;
using DigitalStokvel.GroupService.Repositories;
using DigitalStokvel.GroupService.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DigitalStokvel.GroupService.Tests;

[Trait("Category", "Unit")]
[Trait("Component", "GroupService")]
public class GroupServiceTests
{
    private readonly Mock<IGroupRepository> _mockRepository;
    private readonly GroupService _service;
    
    public GroupServiceTests()
    {
        _mockRepository = new Mock<IGroupRepository>();
        _service = new GroupService(_mockRepository.Object);
    }
    
    [Fact]
    public async Task CreateGroup_WithValidData_ShouldReturnGroupId()
    {
        // Arrange
        var groupName = "Mandela Stokvel";
        var contributionAmount = 500m;
        var expectedId = Guid.NewGuid();
        
        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Group>()))
            .ReturnsAsync(expectedId);
        
        // Act
        var result = await _service.CreateGroupAsync(groupName, contributionAmount);
        
        // Assert
        result.Should().Be(expectedId);
        _mockRepository.Verify(
            r => r.CreateAsync(It.Is<Group>(g => 
                g.Name == groupName && 
                g.ContributionAmount == contributionAmount
            )),
            Times.Once
        );
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task CreateGroup_WithInvalidName_ShouldThrowValidationException(string invalidName)
    {
        // Arrange & Act
        Func<Task> act = async () => await _service.CreateGroupAsync(invalidName, 500m);
        
        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*name*");
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task CreateGroup_WithInvalidAmount_ShouldThrowValidationException(decimal invalidAmount)
    {
        // Arrange & Act
        Func<Task> act = async () => await _service.CreateGroupAsync("Test Group", invalidAmount);
        
        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*amount*");
    }
    
    [Fact]
    public async Task GetGroup_WhenGroupExists_ShouldReturnGroup()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var expectedGroup = new GroupBuilder()
            .WithName("Mandela Stokvel")
            .Build();
        
        _mockRepository
            .Setup(r => r.GetByIdAsync(groupId))
            .ReturnsAsync(expectedGroup);
        
        // Act
        var result = await _service.GetGroupAsync(groupId);
        
        // Assert
        result.Should().BeEquivalentTo(expectedGroup);
    }
    
    [Fact]
    public async Task GetGroup_WhenGroupNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        
        _mockRepository
            .Setup(r => r.GetByIdAsync(groupId))
            .ReturnsAsync((Group)null);
        
        // Act
        Func<Task> act = async () => await _service.GetGroupAsync(groupId);
        
        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{groupId}*");
    }
}
```

---

**Document Status:** Active  
**Last Updated:** March 24, 2026  
**Next Review:** After Phase 2 implementation
