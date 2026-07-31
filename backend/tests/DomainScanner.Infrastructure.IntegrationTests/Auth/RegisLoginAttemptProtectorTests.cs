using DomainScanner.Application.Abstractions.Auth.Models;
using DomainScanner.Contracts.Options;
using DomainScanner.Contracts.Options.Login;
using DomainScanner.Infrastructure.Auth.Authentication.LoginProtection;
using DomainScanner.Infrastructure.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DomainScanner.Infrastructure.IntegrationTests.Auth;

public class RegisLoginAttemptProtectorTests : IClassFixture<RedisFixture>
{
    private readonly RedisFixture _redisFixture;

    private readonly LoginProtectionOptions _options = new()
    {
        KeyPrefix = $"integration-tests:{Guid.NewGuid():N}",
        FailureWindowMinutes = 15,
        LockoutThreshold = 5,
        LockoutDurationMinutes = 10,
        MaximumLockoutMinutes = 60,
        EscalationWindowMinutes = 1440,
        DelayStartAttempt = 3,
        InitialDelayMilliseconds = 500,
        MaximumDelayMilliseconds = 2000
    };

    public RegisLoginAttemptProtectorTests(RedisFixture redisFixture)
    {
        _redisFixture = redisFixture;
    }

    private RedisLoginAttemptProtector CreateProtector(
        Action<LoginProtectionOptions>? configure = null
    )
    {
        configure?.Invoke(_options);
        
        return new RedisLoginAttemptProtector(
            _redisFixture.Connection,
            Options.Create(_options));
    }

    private static string CreateAccountKey()
    {
        return $"auth:login:v1:{Guid.NewGuid():N}";
    }

    [Fact]
    public async Task GetState_NewAccount_ReturnsEmptyState()
    {
        // Arrange
        var protector = CreateProtector();
        var accountKey = CreateAccountKey();

        // Act
        var state = await protector.GetStateAsync(
            accountKey,
            CancellationToken.None
        );

        // Assert
        state.IsBlocked.Should().BeFalse();
        state.FailedAttempts.Should().Be(0);
        state.RetryAfter.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task RegisterFailure_IncrementsCounterAndCalculatesDelay()
    {
        // Arrange
        var protector = CreateProtector();
        var accountKey = CreateAccountKey();

        TimeSpan timeZero = TimeSpan.Zero;
        TimeSpan timeIncrement = TimeSpan.FromMilliseconds(_options.InitialDelayMilliseconds);
        
        // Act
        var first = await protector.RegisterFailureAsync(
            accountKey,
            CancellationToken.None);

        var second = await protector.RegisterFailureAsync(
            accountKey,
            CancellationToken.None);

        var third = await protector.RegisterFailureAsync(
            accountKey,
            CancellationToken.None);

        var fourth = await protector.RegisterFailureAsync(
            accountKey,
            CancellationToken.None);

        // Assert
        first.FailedAttempts.Should().Be(1);
        first.Delay.Should().Be(timeZero);

        second.FailedAttempts.Should().Be(2);
        second.Delay.Should().Be(timeZero);

        third.FailedAttempts.Should().Be(3);
        third.Delay.Should().Be(timeZero + timeIncrement);

        fourth.FailedAttempts.Should().Be(4);
        fourth.Delay.Should().Be(timeZero + (timeIncrement * 2));

        fourth.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterFailure_AtThreshold_BlocksAccount()
    {
        // Arrange
        var protector = CreateProtector();
        var accountKey = CreateAccountKey();

        LoginFailureResult? failureResult = null;

        // Act + Assert
        for (var attempt = 1; attempt <= _options.LockoutThreshold; attempt++)
        {
            failureResult = await protector.RegisterFailureAsync(
                accountKey,
                CancellationToken.None
            );
        }

        failureResult.Should().NotBeNull();
        failureResult.FailedAttempts.Should().Be(5);
        failureResult.IsBlocked.Should().BeTrue();
        failureResult.Delay.Should().Be(TimeSpan.Zero);

        failureResult.RetryAfter.Should().BeCloseTo(
            TimeSpan.FromMinutes(_options.LockoutDurationMinutes),
            TimeSpan.FromSeconds(1)
        );

        var state = await protector.GetStateAsync(
            accountKey,
            CancellationToken.None
        );

        state.IsBlocked.Should().BeTrue();
        state.FailedAttempts.Should().Be(5);
        state.RetryAfter.Should().BeGreaterThan(
            TimeSpan.FromMinutes(_options.LockoutDurationMinutes) -
                TimeSpan.FromSeconds(1)
            );
    }
    
    [Fact]
    public async Task RegisterFailure_AlreadyBlocked_DoesNotIncrementCounter()
    {
        // Arrange
        var protector = CreateProtector();
        var accountKey = CreateAccountKey();
        
        // Act
        for (var attempt = 0; attempt <= _options.LockoutThreshold; attempt++)
        {
            await protector.RegisterFailureAsync(
                accountKey,
                CancellationToken.None
            );
        }

        var result = await protector.GetStateAsync(
            accountKey,
            CancellationToken.None
        );
        
        // Assert
        result.IsBlocked.Should().BeTrue();
        result.FailedAttempts.Should().Be(5);
        result.RetryAfter.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task Reset_RemovesFailuresAndLockout()
    {
        // Arrange
        var protector = CreateProtector();
        var accountKey = CreateAccountKey();

        // Act
        for (var attempt = 0; attempt <= _options.LockoutThreshold; attempt++)
        {
            await protector.RegisterFailureAsync(
                accountKey,
                CancellationToken.None
            );
        }

        await protector.ResetAsync(
            accountKey,
            CancellationToken.None
        );

        var state = await protector.GetStateAsync(
            accountKey,
            CancellationToken.None
        );

        // Assert
        state.IsBlocked.Should().BeFalse();
        state.FailedAttempts.Should().Be(0);
        state.RetryAfter.Should().Be(TimeSpan.Zero);
    }
    
    [Fact]
    public async Task Reset_RemovesLockoutEscalationHistory()
    {
        // Arrange
        var protector = CreateProtector();
        var accountKey = CreateAccountKey();

        LoginFailureResult? secondLockout = null;

        // Act
        for (var attempt = 0; attempt < _options.LockoutThreshold; attempt++)
        {
            await protector.RegisterFailureAsync(
                accountKey,
                CancellationToken.None);
        }

        await protector.ResetAsync(
            accountKey,
            CancellationToken.None);
        
        for (var attempt = 0; attempt < 5; attempt++)
        {
            secondLockout =
                await protector.RegisterFailureAsync(
                    accountKey,
                    CancellationToken.None);
        }

        secondLockout!.RetryAfter.Should().BeCloseTo(
            TimeSpan.FromMinutes(_options.LockoutDurationMinutes),
            TimeSpan.FromSeconds(1)
            );
    }
    
    [Fact]
    public async Task RegisterFailure_FirstFailure_SetsExpiration()
    {
        // Arrange
        const string prefix = "ttl-test";
        var accountKey = CreateAccountKey();

        RedisKey stateKey =
            $"{prefix}:login:state:{{{accountKey}}}";
        
        var protector = CreateProtector(options =>
        {
            options.KeyPrefix = prefix;
        });

        // Act
        await protector.RegisterFailureAsync(
            accountKey,
            CancellationToken.None);

        var ttl = await _redisFixture.Connection
            .GetDatabase()
            .KeyTimeToLiveAsync(stateKey);

        // Assert
        ttl.Should().NotBeNull();
        ttl.Should().BeLessThanOrEqualTo(
            TimeSpan.FromMinutes(_options.FailureWindowMinutes));
        ttl.Should().BeGreaterThan(
            TimeSpan.FromMinutes(14));
    }
    
    [Fact]
    public async Task RegisterFailure_ConcurrentRequests_DoesNotLoseUpdates()
    {
        // Arrange
        var protector = CreateProtector(options =>
        {
            options.LockoutThreshold = 100;
            options.DelayStartAttempt = 50;
        });

        var accountKey = CreateAccountKey();

        var tasks = Enumerable
            .Range(0, 20)
            .Select(_ =>
                protector.RegisterFailureAsync(
                    accountKey,
                    CancellationToken.None));

        // Act
        var results = await Task.WhenAll(tasks);

        var attempts = results
            .Select(result => result.FailedAttempts)
            .Order()
            .ToArray();

        // Assert
        attempts.Should().Equal(
            Enumerable.Range(1, 20));

        var state = await protector.GetStateAsync(
            accountKey,
            CancellationToken.None);

        state.FailedAttempts.Should().Be(20);
        state.IsBlocked.Should().BeFalse();
    }
    
    [Fact]
    public async Task RegisterFailure_DifferentAccounts_HaveIndependentState()
    {
        // Arrange
        var protector = CreateProtector();
        var firstAccount = CreateAccountKey();
        var secondAccount = CreateAccountKey();

        // Act
        await protector.RegisterFailureAsync(
            firstAccount,
            CancellationToken.None);

        await protector.RegisterFailureAsync(
            firstAccount,
            CancellationToken.None);

        await protector.RegisterFailureAsync(
            secondAccount,
            CancellationToken.None);

        var firstState = await protector.GetStateAsync(
            firstAccount,
            CancellationToken.None);

        var secondState = await protector.GetStateAsync(
            secondAccount,
            CancellationToken.None);

        // Assert
        firstState.FailedAttempts.Should().Be(2);
        secondState.FailedAttempts.Should().Be(1);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Methods_EmptyAccountKey_ThrowArgumentException(
        string accountKey)
    {
        // Arrange
        var protector = CreateProtector();
        
        // Act
        var getAction = () => protector.GetStateAsync(
            accountKey,
            CancellationToken.None);

        var failureAction = () => protector.RegisterFailureAsync(
            accountKey,
            CancellationToken.None);

        var resetAction = () => protector.ResetAsync(
            accountKey,
            CancellationToken.None);

        // Assert
        await getAction.Should().ThrowAsync<ArgumentException>();
        await failureAction.Should().ThrowAsync<ArgumentException>();
        await resetAction.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetState_CancelledToken_ThrowsOperationCancelled()
    {
        // Arrange
        var protector = CreateProtector();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var action = () => protector.GetStateAsync(
            CreateAccountKey(),
            cts.Token);

        // Assert
        await action.Should()
            .ThrowAsync<OperationCanceledException>();
    }
}