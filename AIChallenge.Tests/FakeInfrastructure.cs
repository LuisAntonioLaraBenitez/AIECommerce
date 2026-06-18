using AIChallenge.Data;
using AIChallenge.Models;
using AIChallenge.Services;

namespace AIChallenge.Tests;

internal sealed class InMemoryDataStore : IDataStore
{
    private AppData _data;

    public InMemoryDataStore(AppData data)
    {
        _data = data;
    }

    public Task<AppData> ReadAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_data);
    }

    public Task WriteAsync(AppData data, CancellationToken cancellationToken = default)
    {
        _data = data;
        return Task.CompletedTask;
    }
}

internal sealed class CapturingPurchaseLogger : IPurchaseLogger
{
    public List<PurchaseAttemptLog> Logs { get; } = [];

    public Task LogAsync(PurchaseAttemptLog log, CancellationToken cancellationToken = default)
    {
        Logs.Add(log);
        return Task.CompletedTask;
    }
}

internal sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FixedTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _utcNow;
    }
}
