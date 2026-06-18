using AIChallenge.Models;

namespace AIChallenge.Services;

public interface IPurchaseLogger
{
    Task LogAsync(PurchaseAttemptLog log, CancellationToken cancellationToken = default);
}
