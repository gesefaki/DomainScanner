namespace DomainScanner.Contracts.Exceptions.Domains;

/// <summary>The requested check does not exist for the owned domain.</summary>
public sealed class DomainCheckNotFoundException : Exception
{
    /// <summary>Creates a missing-check exception.</summary>
    public DomainCheckNotFoundException(Guid checkId)
        : base($"Domain check {checkId} was not found.")
    {
    }
}
