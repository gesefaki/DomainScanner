namespace DomainScanner.Contracts.Models;

public sealed record ApiError(
    string Code,
    string Message,
    string TraceId,
    IReadOnlyDictionary<string, string[]>? Errors = null);