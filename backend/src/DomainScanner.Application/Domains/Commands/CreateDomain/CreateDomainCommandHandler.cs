using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Users.Exceptions;

namespace DomainScanner.Application.Domains.Commands.CreateDomain;

public class CreateDomainCommandHandler(IDomainsRepository domainsRepository, IUnitOfWork  unitOfWork,
    IUsersRepository usersRepository) 
    : IRequestHandler<CreateDomainCommand, Guid>
{
    private readonly IDomainsRepository _domainsRepository = domainsRepository;
    private readonly IUsersRepository _usersRepository = usersRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
    public async Task<Guid> Handle(CreateDomainCommand request, CancellationToken ct)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // 1. Get domain from request
            var domain = request.Domain;

            // 2. Trying to find user (if not - 400)
            var user = await _usersRepository.GetUserByIdAsync(domain.UserId, ct);
            if (user is null)
                throw new BadRequestException(request.Domain.UserId);
            
            // 3. Creating domain
            domain.User = user;
            await _domainsRepository.CreateAsync(domain, ct);
            
            // 4. Updating user
            user.Domains.Add(domain);
            _usersRepository.Update(user);

            // 6. Commiting transaction
            await _unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            
            return domain.Id;
        }
        catch (OperationCanceledException ex)
        {
            await transaction.RollbackAsync(ct);
            throw new OperationCanceledException(ex.Message, ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}