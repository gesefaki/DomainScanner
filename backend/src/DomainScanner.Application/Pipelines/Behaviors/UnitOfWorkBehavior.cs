using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Pipelines.Interfaces;
using MediatR;

namespace DomainScanner.Application.Pipelines.Behaviors;

/// <summary>
/// Pipeline behavior responsible for Unit of Work pattern implementation.
/// </summary>
/// <typeparam name="TRequest">Type of the request.</typeparam>
/// <typeparam name="TResponse">Type of the response.</typeparam>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : ICommand<TResponse>
{
    private readonly IUnitOfWork _uow;

    public UnitOfWorkBehavior(IUnitOfWork uow)
    {
        _uow = uow;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Non-transaction behavior (only saving)
        if (!IsTransaction(request))
        {
            var response = await next(ct);
            await _uow.SaveChangesAsync(ct);
            return response;
        }

        // Transaction behavior
        try
        {
            await _uow.BeginTransactionAsync(ct);

            var response = await next(ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            return response;
        }
        catch (Exception)
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private bool IsTransaction(TRequest request)
    {
        return request is ITransaction<TResponse>;
    }
}