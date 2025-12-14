namespace PilotLife.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-related errors.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// A unique code identifying the type of error.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// The HTTP status code that should be returned when this exception is thrown.
    /// </summary>
    public int StatusCode { get; }

    protected DomainException(string code, string message, int statusCode = 500)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    protected DomainException(string code, string message, int statusCode, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
