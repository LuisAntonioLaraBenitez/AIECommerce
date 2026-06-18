using System.Text.Json;
using AIChallenge.Models;
using AIChallenge.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AIChallenge.Tests;

public sealed class PurchaseLoggerTests
{
    [Fact]
    public async Task LogAsync_AppendsJsonLine_AndWritesInformationLog()
    {
        using TestWebHostEnvironment env = new();
        CapturingLogger<PurchaseLogger> logger = new();
        PurchaseLogger sut = new(env, logger);
        PurchaseAttemptLog attempt = new(
            "LOG-1",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            "CUS-1",
            "PAY-1",
            100m,
            ["SKU-1"],
            true,
            "SIM-123456",
            null);

        await sut.LogAsync(attempt);

        string file = Path.Combine(env.ContentRootPath, "Logs", "purchase-attempts.jsonl");
        Assert.True(File.Exists(file));
        string[] lines = await File.ReadAllLinesAsync(file);
        Assert.Single(lines);
        PurchaseAttemptLog? parsed = JsonSerializer.Deserialize<PurchaseAttemptLog>(lines[0], new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(parsed);
        Assert.Equal("LOG-1", parsed.Id);
        Assert.Contains(logger.Messages, m => m.Contains("Purchase attempt LOG-1 for customer CUS-1", StringComparison.Ordinal));
    }
}

internal sealed class CapturingLogger<T> : ILogger<T>
{
    public List<string> Messages { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Messages.Add(formatter(state, exception));
    }
}
