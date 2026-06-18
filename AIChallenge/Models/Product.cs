namespace AIChallenge.Models;

public sealed record Product(
    string Sku,
    string Name,
    decimal Price,
    IReadOnlyList<string> Features);
