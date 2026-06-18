namespace AIChallenge.Models;

public sealed record PaymentMethod(
    string Id,
    string CustomerId,
    string MaskedCardNumber,
    string CardFingerprint,
    CardType CardType,
    string CardholderName,
    string Expiration,
    DateTimeOffset CreatedAt);
