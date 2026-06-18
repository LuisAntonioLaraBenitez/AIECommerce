using System.Text.Json;
using AIChallenge.Models;

namespace AIChallenge.Services;

public interface IPurchaseLogger
{
    Task LogAsync(PurchaseAttemptLog log, CancellationToken cancellationToken = default);
}

public sealed class PurchaseLogger : IPurchaseLogger
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string _logPath;
    private readonly ILogger<PurchaseLogger> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public PurchaseLogger(IWebHostEnvironment environment, ILogger<PurchaseLogger> logger)
    {
        string logsDirectory = Path.Combine(environment.ContentRootPath, "Logs");
        Directory.CreateDirectory(logsDirectory);
        _logPath = Path.Combine(logsDirectory, "purchase-attempts.jsonl");
        _logger = logger;
    }

    public async Task LogAsync(PurchaseAttemptLog log, CancellationToken cancellationToken = default)
    {
        string json = JsonSerializer.Serialize(log, JsonOptions);
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(_logPath, json + Environment.NewLine, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        _logger.LogInformation("Purchase attempt {AttemptId} for customer {CustomerId}: accepted={Accepted}, total={Total}, authorization={AuthorizationCode}",
            log.Id,
            log.CustomerId,
            log.Accepted,
            log.Total,
            log.AuthorizationCode);
    }
}
