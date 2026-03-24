# Digital Stokvel Banking - Coding Standards

**Version:** 1.0  
**Last Updated:** March 2026

## General Principles

1. **Code Quality**: Write clean, maintainable, and testable code
2. **SOLID Principles**: Follow SOLID design principles
3. **DRY**: Don't Repeat Yourself - extract common functionality
4. **KISS**: Keep It Simple, Stupid - avoid over-engineering
5. **YAGNI**: You Aren't Gonna Need It - don't add speculative functionality

## C# / .NET Standards

### Naming Conventions

- **Classes, Methods, Properties**: PascalCase
  ```csharp
  public class GroupService { }
  public void CreateGroup() { }
  public string GroupName { get; set; }
  ```

- **Private fields**: _camelCase with underscore prefix
  ```csharp
  private readonly ILogger<GroupService> _logger;
  private string _connectionString;
  ```

- **Local variables, parameters**: camelCase
  ```csharp
  var groupId = Guid.NewGuid();
  public void ProcessContribution(decimal amount, string memberId) { }
  ```

- **Constants**: PascalCase
  ```csharp
  public const int MaxGroupMembers = 50;
  ```

- **Interfaces**: PascalCase with 'I' prefix
  ```csharp
  public interface IGroupRepository { }
  ```

### Code Structure

- **File Organization**: One class per file, file name matches class name
- **Namespace**: Match folder structure
  ```csharp
  namespace DigitalStokvel.GroupService.Domain.Entities;
  ```

- **Using Directives**: 
  - System namespaces first
  - Third-party namespaces
  - Project namespaces
  - Use implicit usings where appropriate

### Nullable Reference Types

- Enable nullable reference types (`<Nullable>enable</Nullable>`)
- Use `?` suffix for nullable reference types explicitly when needed
- Avoid null-forgiving operator (`!`) unless absolutely necessary

```csharp
public string? OptionalDescription { get; set; }
public string RequiredName { get; set; } = string.Empty;
```

### Async/Await

- Use async/await for I/O-bound operations
- Suffix async methods with `Async`
- Always await async calls, don't use `.Result` or `.Wait()`

```csharp
public async Task<Group> GetGroupAsync(Guid groupId, CancellationToken cancellationToken)
{
    return await _repository.FindByIdAsync(groupId, cancellationToken);
}
```

### Exception Handling

- Use specific exception types
- Include meaningful error messages
- Log exceptions with context
- Don't catch exceptions you can't handle

```csharp
try
{
    var group = await _repository.GetGroupAsync(groupId);
}
catch (EntityNotFoundException ex)
{
    _logger.LogWarning(ex, "Group {GroupId} not found", groupId);
    throw;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error retrieving group {GroupId}", groupId);
    throw;
}
```

### Dependency Injection

- Use constructor injection for required dependencies
- Inject interfaces, not concrete implementations
- Keep constructors simple - don't perform logic

```csharp
public class GroupService
{
    private readonly IGroupRepository _repository;
    private readonly ILogger<GroupService> _logger;

    public GroupService(IGroupRepository repository, ILogger<GroupService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

### Logging

- Use structured logging with log levels appropriately
- Include correlation IDs for distributed tracing
- Don't log sensitive data (PII, passwords, tokens)

```csharp
_logger.LogInformation(
    "Group {GroupId} created by user {UserId}",
    group.Id,
    userId);
```

### API Controllers

- Use `[ApiController]` attribute
- Use attribute routing
- Return appropriate HTTP status codes
- Use DTOs for request/response models

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class GroupsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(GroupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        // Implementation
    }
}
```

## Database Standards

### Entity Framework Core

- Use migrations for schema changes
- Implement `IEntityTypeConfiguration` for entity configuration
- Use value objects for complex types
- Enable query splitting for collections

```csharp
public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(200);
    }
}
```

### Repository Pattern

- Use repository pattern for data access
- Return domain entities from repositories
- Keep repositories focused on data access only

## Testing Standards

- **Unit Tests**: Test individual methods in isolation
- **Integration Tests**: Test service interactions
- **Test Coverage**: Aim for 80%+ code coverage
- **Naming**: `MethodName_StateUnderTest_ExpectedBehavior`

```csharp
[Fact]
public async Task CreateGroup_WithValidData_ReturnsCreatedGroup()
{
    // Arrange
    var request = new CreateGroupRequest { Name = "Test Group" };
    
    // Act
    var result = await _service.CreateGroupAsync(request);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test Group", result.Name);
}
```

## Security Standards

- Never hardcode secrets or credentials
- Use environment variables or Azure Key Vault
- Validate all input data
- Sanitize output data
- Use parameterized queries (EF Core does this by default)
- Implement rate limiting on APIs
- Use HTTPS/TLS for all communications

## Performance Standards

- Use `AsNoTracking()` for read-only queries
- Implement caching for frequently accessed data
- Use pagination for large datasets
- Optimize database queries (avoid N+1 problems)
- Use background jobs for long-running tasks

```csharp
var groups = await _context.Groups
    .AsNoTracking()
    .Where(g => g.IsActive)
    .OrderBy(g => g.Name)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

## Documentation Standards

- Use XML documentation comments for public APIs
- Document complex business logic
- Keep comments up-to-date with code changes
- Use README files for project/module overviews

```csharp
/// <summary>
/// Creates a new stokvel group with the specified members and constitution.
/// </summary>
/// <param name="request">The group creation request containing group details.</param>
/// <param name="cancellationToken">Cancellation token for async operation.</param>
/// <returns>The created group with assigned ID and timestamps.</returns>
/// <exception cref="ValidationException">Thrown when request data is invalid.</exception>
public async Task<Group> CreateGroupAsync(
    CreateGroupRequest request,
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

## Git Commit Standards

- Use conventional commits format
- Write clear, descriptive commit messages
- Reference ticket/task numbers

```
feat(group-service): add group creation endpoint

Implements POST /api/v1/groups endpoint with validation and persistence.
Relates to task 2.2.2

- Add CreateGroupRequest DTO
- Implement validation logic
- Add unit tests for controller
```

## Code Review Checklist

- [ ] Code follows naming conventions
- [ ] All public APIs have XML documentation
- [ ] Unit tests are written and passing
- [ ] No hardcoded secrets or sensitive data
- [ ] Error handling is appropriate
- [ ] Logging includes necessary context
- [ ] Code is DRY and SOLID compliant
- [ ] Performance considerations addressed
- [ ] Security best practices followed

---

**Note**: These standards are living guidelines and will evolve as the project grows. When in doubt, prioritize clarity and maintainability over cleverness.
