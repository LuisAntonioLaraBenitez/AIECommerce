namespace AIChallenge.Models;

public sealed record CreateOrderItemRequest(
    string Sku,
    int Quantity);
