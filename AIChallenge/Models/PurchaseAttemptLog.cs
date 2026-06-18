namespace AIChallenge.Models;

public sealed record PurchaseAttemptLog(
    string Id,
    DateTimeOffset Timestamp,
    string CustomerId,
    string? PaymentMethodId,
    decimal Total,
    IReadOnlyList<string> ProductSkus,
    bool Accepted,
    string AuthorizationCode,
    string? RejectionReason);
