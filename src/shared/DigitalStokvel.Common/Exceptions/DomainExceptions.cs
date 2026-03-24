namespace DigitalStokvel.Common.Exceptions;

/// <summary>
/// Base exception for all Digital Stokvel domain exceptions
/// </summary>
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }

    protected DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    protected DomainException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown when an entity is not found
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, Guid id)
        : base($"{entityName} with ID '{id}' was not found", "ENTITY_NOT_FOUND")
    {
    }

    public EntityNotFoundException(string entityName, string identifier)
        : base($"{entityName} with identifier '{identifier}' was not found", "ENTITY_NOT_FOUND")
    {
    }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message)
        : base(message, "BUSINESS_RULE_VIOLATION")
    {
    }

    public BusinessRuleViolationException(string message, Exception innerException)
        : base(message, "BUSINESS_RULE_VIOLATION", innerException)
    {
    }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred", "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base(errorMessage, "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }
}

/// <summary>
/// Exception thrown when an entity already exists
/// </summary>
public class DuplicateEntityException : DomainException
{
    public DuplicateEntityException(string entityName, string identifier)
        : base($"{entityName} with identifier '{identifier}' already exists", "DUPLICATE_ENTITY")
    {
    }
}

/// <summary>
/// Exception thrown when an operation is not authorized
/// </summary>
public class UnauthorizedOperationException : DomainException
{
    public UnauthorizedOperationException(string operation)
        : base($"Unauthorized to perform operation: {operation}", "UNAUTHORIZED")
    {
    }
}

/// <summary>
/// Exception thrown when external service call fails
/// </summary>
public class ExternalServiceException : DomainException
{
    public string ServiceName { get; }

    public ExternalServiceException(string serviceName, string message)
        : base($"External service '{serviceName}' failed: {message}", "EXTERNAL_SERVICE_ERROR")
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException)
        : base($"External service '{serviceName}' failed: {message}", "EXTERNAL_SERVICE_ERROR", innerException)
    {
        ServiceName = serviceName;
    }
}

/// <summary>
/// Exception thrown when payment processing fails
/// </summary>
public class PaymentProcessingException : DomainException
{
    public PaymentProcessingException(string message)
        : base(message, "PAYMENT_FAILED")
    {
    }

    public PaymentProcessingException(string message, Exception innerException)
        : base(message, "PAYMENT_FAILED", innerException)
    {
    }
}
