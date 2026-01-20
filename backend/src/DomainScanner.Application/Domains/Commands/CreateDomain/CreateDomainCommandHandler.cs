using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Domains.Commands.CreateDomain;

public class CreateDomainCommandHandler : IRequestHandler<CreateDomainCommand, Guid>
{
    private readonly IUnitOfWork _uof;
    private readonly IUsersRepository _usersRepository;
    private readonly IDomainsRepository _domainsRepository;
    private readonly ILogger<CreateDomainCommandHandler> _logger;

    public CreateDomainCommandHandler(IUnitOfWork uof, IUsersRepository usersRepository,
        IDomainsRepository domainsRepository, ILogger<CreateDomainCommandHandler> logger)
    {
        _uof = uof;
        _usersRepository = usersRepository;
        _domainsRepository = domainsRepository;
        _logger = logger;
    }
    
    public async Task<Guid> Handle(CreateDomainCommand request, CancellationToken ct)
    {
        await using var transaction = await _uof.BeginTransactionAsync(ct);

        try
        {
                // Get domain from request
                var domain = request.Domain;
                if (!domain.Address.StartsWith("http") || !domain.Address.StartsWith("https"))
                    throw new InvalidAddressFormatException(request.Domain.Address);

                // Trying to find user (if not - 400)
                _logger.LogInformation($"Getting user with id {domain.Id}");
                var user = await _usersRepository.GetUserByIdAsync(domain.UserId, ct);
                if (user is null)
                {
                    _logger.LogWarning($"User with id {domain.Id} not found");
                    throw new UserNotFoundException("no user found", domain.Id);
                }
                _logger.LogInformation($"User with id {user.Id}: {user.Username} was find");

                // Creating domain
                _logger.LogInformation($"Creating domain {domain.Id}");
                await _domainsRepository.CreateAsync(domain, ct);
                _logger.LogInformation($"Domain {domain.Id} created");
            
                // Updating user
                _logger.LogInformation($"Updating user with id {domain.Id}");
                
                user.Domains.Add(domain);
                _usersRepository.Update(user);
                _logger.LogInformation($"User with id {user.Id} was created");

                // Commiting transaction
                await _uof.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                _logger.LogInformation("Operation is successful");
            
                return domain.Id;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning("Operation was canceled");
            await transaction.RollbackAsync(ct);
            throw new OperationCanceledException(ex.Message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError("ERROR: " +  ex.Message);
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}