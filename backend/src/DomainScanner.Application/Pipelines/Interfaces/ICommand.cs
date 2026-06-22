using MediatR;

namespace DomainScanner.Application.Behaviors;

public interface ICommand<TResponse> : IRequest<TResponse>;