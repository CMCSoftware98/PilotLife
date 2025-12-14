namespace PilotLife.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string resource, Guid id)
        : base("NOT_FOUND", $"{resource} with ID {id} was not found", 404)
    {
    }

    public NotFoundException(string resource, string identifier)
        : base("NOT_FOUND", $"{resource} '{identifier}' was not found", 404)
    {
    }

    public NotFoundException(string message)
        : base("NOT_FOUND", message, 404)
    {
    }
}
