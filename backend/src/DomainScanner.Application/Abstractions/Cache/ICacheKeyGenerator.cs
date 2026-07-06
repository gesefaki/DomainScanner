namespace DomainScanner.Application.Abstractions.Cache;

public interface ICacheKeyGenerator<T> where T : notnull
{
    string GenerateKey(T request);
}