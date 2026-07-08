using MediatR;

namespace DomainScanner.Application.Pipelines.Interfaces;

/// <summary>
/// Marks request as command. 
/// </summary>
/// <typeparam name="TResponse">Type of response returned after executing the command.</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse>;