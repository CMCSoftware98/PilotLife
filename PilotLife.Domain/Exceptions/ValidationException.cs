namespace PilotLife.Domain.Exceptions;

/// <summary>
/// Represents a single validation error.
/// </summary>
public record ValidationError(string Field, string Message);

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : DomainException
{
    /// <summary>
    /// The collection of validation errors.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationException(IEnumerable<ValidationError> errors)
        : base("VALIDATION_ERROR", "One or more validation errors occurred", 400)
    {
        Errors = errors.ToList();
    }

    public ValidationException(string field, string message)
        : base("VALIDATION_ERROR", message, 400)
    {
        Errors = new List<ValidationError> { new(field, message) };
    }

    public ValidationException(string message)
        : base("VALIDATION_ERROR", message, 400)
    {
        Errors = new List<ValidationError>();
    }
}
