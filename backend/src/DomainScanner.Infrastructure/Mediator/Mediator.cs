using DomainScanner.Application.Abstractions.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace DomainScanner.Infrastructure.Mediator;

public class Mediator : IMediator
{
    private readonly IServiceProvider _provider;

    public Mediator(IServiceProvider provider)
    {
        _provider = provider;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct)
    {
        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(request.GetType(), typeof(TResponse));
        
        dynamic handler = _provider.GetRequiredService(handlerType);
        
        return handler.Handle((dynamic)request, ct);
    }
}