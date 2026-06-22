using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Behaviors;
using MediatR;

namespace DomainScanner.Application.Pipelines;

public class UnitOfWorkBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : ICommand<TRequest>
{
    private IUnitOfWork _uow;

    public UnitOfWorkBehavior(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var response = await next(ct);

        await _uow.SaveChangesAsync(ct);

        return response;
    }
}