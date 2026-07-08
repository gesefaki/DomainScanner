namespace DomainScanner.Application.Pipelines.Interfaces;

/// <summary>
/// Marks command as requiring a transaction. Implements in <see cref="UnitOfWorkBehavior{TRequest, TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">Type of response returned after executing the command.</typeparam>
public interface ITransaction<TResponse> : ICommand<TResponse>;