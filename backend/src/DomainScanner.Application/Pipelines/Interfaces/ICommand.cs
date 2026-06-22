using MediatR;

namespace DomainScanner.Application.Pipelines.Interfaces;

public interface ICommand<TResponse> : IRequest<TResponse>;