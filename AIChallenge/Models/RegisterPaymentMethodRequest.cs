namespace AIChallenge.Models;

public sealed record RegisterPaymentMethodRequest(
    string CustomerId,
    string CardNumber,
    CardType CardType,
    string CardholderName,
    string Expiration,
    string Cvv);
