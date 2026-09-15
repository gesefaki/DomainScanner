using System.Linq.Expressions;
using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Domains.Queries.GetDomainById;
using DomainScanner.Application.Pipelines.Behaviors;
using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Exceptions.Common;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Queries;

/// <summary>
/// Tests ownership, authentication, and the ordered check history returned by the domain query.
/// </summary>
public class GetDomainByIdQueryHandlerTests
{
    private readonly Mock<IReadRepository<DomainEntity, Guid>> _repository = new(MockBehavior.Strict);
    private readonly Mock<IReadRepository<DomainCheckResult, Guid>> _checks = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _domainId = Guid.NewGuid();
    private readonly GetDomainByIdQueryHandler _handler;

    public GetDomainByIdQueryHandlerTests()
    {
        _currentUser.SetupGet(user => user.Id).Returns(_userId);
        _currentUser.SetupGet(user => user.IsAuthenticated).Returns(true);
        _handler = new GetDomainByIdQueryHandler(
            _repository.Object, _checks.Object, _mapper.Object, _currentUser.Object);
    }

    /// <summary>
    /// The owner receives domain fields and checks loaded by DomainId, newest first with descending
    /// identifiers breaking ties. Navigation collections and checks for other domains are excluded.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDomainBelongsToCurrentUser_ReturnsOrderedHistoryFromCheckRepository()
    {
        var domain = new DomainBuilder().WithId(_domainId).WithUserId(_userId).Inactive().Build();
        domain.CheckResults.Add(new DomainCheckResult { Address = "navigation-only.example" });
        var createdAt = new DateTime(2026, 9, 15, 12, 0, 0, DateTimeKind.Utc);
        var older = CreateCheck("00000000-0000-0000-0000-000000000003", _domainId, createdAt.AddMinutes(-1), 200, true);
        var smallerId = CreateCheck("00000000-0000-0000-0000-000000000001", _domainId, createdAt, 503, false);
        var largerId = CreateCheck("00000000-0000-0000-0000-000000000002", _domainId, createdAt, 201, true);
        var foreign = CreateCheck("00000000-0000-0000-0000-000000000004", Guid.NewGuid(), createdAt.AddMinutes(1), 200, true);
        DomainCheckResult[] allChecks = [older, smallerId, foreign, largerId];
        using var cts = new CancellationTokenSource();
        _repository.Setup(repository => repository.FindAsync(_domainId, cts.Token)).ReturnsAsync(domain);
        _checks.Setup(repository => repository.GetAllWhereAsync(
                It.IsAny<Expression<Func<DomainCheckResult, bool>>>(), cts.Token))
            .ReturnsAsync((Expression<Func<DomainCheckResult, bool>> predicate, CancellationToken _) =>
                allChecks.Where(predicate.Compile()).ToArray());

        var response = await _handler.Handle(new GetDomainByIdQuery(_domainId), cts.Token);

        Assert.Equal(domain.Id, response.Id);
        Assert.Equal(domain.UserId, response.UserId);
        Assert.Equal(domain.Address, response.Address);
        Assert.False(response.IsAvailable);
        var history = response.Checks.ToArray();
        Assert.Equal(new[] { largerId.Address, smallerId.Address, older.Address }, history.Select(check => check.Address));
        Assert.Equal(new[] { 201, 503, 200 }, history.Select(check => check.StatusCode));
        Assert.Equal(new[] { true, false, true }, history.Select(check => check.IsSuccess));
        Assert.Equal(new[] { createdAt, createdAt, older.CreatedAt }, history.Select(check => check.CreatedAt));
        _checks.Verify(repository => repository.GetAllWhereAsync(
            It.IsAny<Expression<Func<DomainCheckResult, bool>>>(), cts.Token), Times.Once);
        _mapper.VerifyNoOtherCalls();
    }

    /// <summary>
    /// A domain without stored checks returns an empty history.
    /// </summary>
    [Fact]
    public async Task Handle_WhenNoChecksExist_ReturnsEmptyHistory()
    {
        var domain = new DomainBuilder().WithId(_domainId).WithUserId(_userId).Build();
        _repository.Setup(repository => repository.FindAsync(_domainId, CancellationToken.None)).ReturnsAsync(domain);
        _checks.Setup(repository => repository.GetAllWhereAsync(
                It.IsAny<Expression<Func<DomainCheckResult, bool>>>(), CancellationToken.None))
            .ReturnsAsync(Array.Empty<DomainCheckResult>());

        var response = await _handler.Handle(new GetDomainByIdQuery(_domainId), CancellationToken.None);

        Assert.Empty(response.Checks);
    }

    /// <summary>
    /// Missing and foreign domains return the same not-found error without loading history,
    /// even when the foreign domain identifier equals the current user identifier.
    /// </summary>
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Handle_WhenDomainIsMissingOrForeign_ThrowsWithoutLoadingHistory(bool exists, bool idEqualsUserId)
    {
        var id = idEqualsUserId ? _userId : _domainId;
        var domain = exists ? new DomainBuilder().WithId(id).WithUserId(Guid.NewGuid()).Build() : null;
        _repository.Setup(repository => repository.FindAsync(id, CancellationToken.None)).ReturnsAsync(domain);

        await Assert.ThrowsAsync<DomainNotFoundException>(
            () => _handler.Handle(new GetDomainByIdQuery(id), CancellationToken.None));

        _checks.VerifyNoOtherCalls();
        _mapper.VerifyNoOtherCalls();
    }

    /// <summary>
    /// The authentication pipeline rejects anonymous requests before the handler runs.
    /// The query does not participate in the shared query cache.
    /// </summary>
    [Fact]
    public async Task Query_RequiresAuthenticationAndDoesNotUseQueryCache()
    {
        var query = new GetDomainByIdQuery(_domainId);
        Assert.IsAssignableFrom<INeedAuthentication>(query);
        Assert.False(typeof(ICacheableQuery).IsAssignableFrom(query.GetType()));
        _currentUser.SetupGet(user => user.IsAuthenticated).Returns(false);
        var behavior = new AuthenticationBehavior<GetDomainByIdQuery, DomainResponse>(_currentUser.Object);

        await Assert.ThrowsAsync<NonAuthenticatedException>(() => behavior.Handle(
            query, ct => _handler.Handle(query, ct), CancellationToken.None));

        _repository.VerifyNoOtherCalls();
        _checks.VerifyNoOtherCalls();
    }

    private static DomainCheckResult CreateCheck(string id, Guid domainId, DateTime createdAt, int statusCode, bool isActive) =>
        new()
        {
            Id = Guid.Parse(id),
            DomainId = domainId,
            Address = $"https://example.com/{id}",
            CreatedAt = createdAt,
            StatusCode = statusCode,
            IsActive = isActive
        };
}
