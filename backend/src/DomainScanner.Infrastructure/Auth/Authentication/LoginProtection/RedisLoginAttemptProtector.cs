using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Auth.Models;
using DomainScanner.Contracts.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DomainScanner.Infrastructure.Auth.Authentication.LoginProtection;

public class RedisLoginAttemptProtector : ILoginAttemptProtector
{
    private readonly IDatabase _database;
    private readonly LoginProtectionOptions _options;

    public RedisLoginAttemptProtector(
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<LoginProtectionOptions> options)
    {
        _database = connectionMultiplexer.GetDatabase();
        _options = options.Value;
    }

    private static readonly LuaScript GetStateScript = LuaScript.Prepare(
        """
        local failures = tonumber(
            redis.call('HGET', @stateKey, 'failures') or '0'
            )
            
        local blockedUntil = tonumber(
            redis.call('HGET', @stateKey, 'blocked_until_ms') or '0'
            )
            
        if blockedUntil == 0 then
            return { failures, 0, 0 }
        end

        local redisTime = redis.call('TIME')
        local nowMs =
            tonumber(redisTime[1]) * 1000 +
            math.floor(tonumber(redisTime[2]) / 1000)

        local retryAfterMs = blockedUntil - nowMs

        if retryAfterMs > 0 then
            return { failures, 1, retryAfterMs }
        end

        return { failures, 0, 0 }
        """);

    private static readonly LuaScript RegisterFailureScript =
        LuaScript.Prepare(
            """
            local redisTime = redis.call('TIME')
            local nowMs =
                tonumber(redisTime[1]) * 1000 +
                math.floor(tonumber(redisTime[2]) / 1000)

            local failureWindowMs = tonumber(@failureWindowMs)
            local lockoutThreshold = tonumber(@lockoutThreshold)
            local baseLockoutMs = tonumber(@baseLockoutMs)
            local maximumLockoutMs = tonumber(@maximumLockoutMs)
            local escalationWindowMs = tonumber(@escalationWindowMs)

            local blockedUntil = tonumber(
                redis.call('HGET', @stateKey, 'blocked_until_ms') or '0'
            )

            local currentFailures = tonumber(
                redis.call('HGET', @stateKey, 'failures') or '0'
            )

            if blockedUntil > nowMs then
                return {
                    currentFailures,
                    1,
                    blockedUntil - nowMs
                }
            end

            local failures =
                redis.call('HINCRBY', @stateKey, 'failures', 1)

            if failures < lockoutThreshold then
                local ttl = redis.call('PTTL', @stateKey)

                if ttl < 0 then
                    redis.call(
                        'PEXPIRE',
                        @stateKey,
                        failureWindowMs
                    )
                end

                return { failures, 0, 0 }
            end

            local strikes = redis.call('INCR', @strikesKey)

            redis.call(
                'PEXPIRE',
                @strikesKey,
                escalationWindowMs
            )

            local lockoutMs = baseLockoutMs

            for strike = 2, strikes do
                lockoutMs = lockoutMs * 2

                if lockoutMs >= maximumLockoutMs then
                    lockoutMs = maximumLockoutMs
                    break
                end
            end

            if lockoutMs > maximumLockoutMs then
                lockoutMs = maximumLockoutMs
            end

            local newBlockedUntil = nowMs + lockoutMs

            redis.call(
                'HSET',
                @stateKey,
                'failures',
                failures,
                'blocked_until_ms',
                newBlockedUntil
            )

            redis.call(
                'PEXPIRE',
                @stateKey,
                lockoutMs
            )

            return { failures, 1, lockoutMs }
            """);

    /// <inheritdoc />
    public async Task<LoginAttemptState> GetStateAsync(string accountKey, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountKey);
        ct.ThrowIfCancellationRequested();

        var keys = CreateKeys(accountKey);

        var result = await _database
            .ScriptEvaluateAsync(
                GetStateScript,
                new
                {
                    stateKey = keys.State
                })
            .WaitAsync(ct);

        var values = ReadArray(result, expectedLength: 3);

        var failedAttempts = checked((int)ReadInt64(values[0]));
        var isBlocked = ReadInt64(values[1]) == 1;
        var retryAfterMs = ReadInt64(values[2]);

        return new LoginAttemptState(
            IsBlocked: isBlocked,
            FailedAttempts: failedAttempts,
            RetryAfter: TimeSpan.FromMilliseconds(
                Math.Max(0, retryAfterMs))
        );
    }

    public async Task<LoginFailureResult> RegisterFailureAsync(string accountKey, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountKey);
        ct.ThrowIfCancellationRequested();

        var keys = CreateKeys(accountKey);

        var result = await _database
            .ScriptEvaluateAsync(
                RegisterFailureScript,
                new
                {
                    stateKey = keys.State,
                    strikesKey = keys.Strikes,

                    failureWindowMs =
                        ToMilliseconds(_options.FailureWindowMinutes),

                    lockoutThreshold =
                        _options.LockoutThreshold,

                    baseLockoutMs =
                        ToMilliseconds(_options.LockoutDurationMinutes),

                    maximumLockoutMs =
                        ToMilliseconds(_options.MaximumLockoutMinutes),

                    escalationWindowMs =
                        ToMilliseconds(_options.EscalationWindowMinutes)
                })
            .WaitAsync(ct);

        var values = ReadArray(result, expectedLength: 3);

        var failedAttempts = checked((int)ReadInt64(values[0]));

        var isBlocked = ReadInt64(values[1]) == 1;

        var retryAfterMs = ReadInt64(values[2]);

        var delay = isBlocked
            ? TimeSpan.Zero 
            : CalculateDelay(failedAttempts);

        return new LoginFailureResult(
            FailedAttempts: failedAttempts,
            IsBlocked: isBlocked,
            Delay: delay,
            RetryAfter: TimeSpan.FromMilliseconds(
                Math.Max(0, retryAfterMs))
        );
    }

    public async Task ResetAsync(string accountKey, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountKey);
        ct.ThrowIfCancellationRequested();

        var keys = CreateKeys(accountKey);

        await _database
            .KeyDeleteAsync([
                keys.State,
                keys.Strikes
            ])
            .WaitAsync(ct);
    }

    private (RedisKey State, RedisKey Strikes) CreateKeys(string accountKey)
    {
        var hashTag = $"{{{accountKey}}}";

        return (
            State:
            $"{_options.KeyPrefix}:login:state:{hashTag}",

            Strikes:
            $"{_options.KeyPrefix}:login:strikes:{hashTag}"
        );

    }

    private TimeSpan CalculateDelay(int failedAttempts)
    {
        if (failedAttempts < _options.DelayStartAttempt)
        {
            return TimeSpan.Zero;
        }

        var exponent =
            failedAttempts - _options.DelayStartAttempt;

        var delayMilliseconds =
            _options.InitialDelayMilliseconds * Math.Pow(2, exponent);

        var cappedMilliseconds = Math.Min(
            delayMilliseconds,
            _options.MaximumDelayMilliseconds
        );

        return TimeSpan.FromMilliseconds(cappedMilliseconds);
    }

    private static RedisResult[] ReadArray(
        RedisResult result,
        int expectedLength
    )
    {
        var values = (RedisResult[]?)result;

        if (values == null || values.Length != expectedLength)
        {
            throw new InvalidOperationException(
                "Redis returned an invalid login protection result.");
        }

        return values;
    }

    private static long ReadInt64(RedisResult result)
    {
        if (!long.TryParse(result.ToString(), out var value))
        {
            throw new InvalidOperationException(
                "Redis returned a non-integer login protection value.");
        }

        return value;
    }

    private static long ToMilliseconds(int minutes)
    {
        return checked((long)TimeSpan
            .FromMinutes(minutes)
            .TotalMilliseconds);
    }
}
