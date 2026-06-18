namespace AIChallenge.Models;

public sealed record OrderItem(
    string Sku,
    string ProductName,
    decimal UnitPrice,
    int Quantity);
