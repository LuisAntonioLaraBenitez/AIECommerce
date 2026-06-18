namespace AIChallenge.Models;

public sealed record PurchaseOrder(
    string Id,
    string CustomerId,
    DateTimeOffset Date,
    decimal Total,
    IReadOnlyList<OrderItem> Products,
    string AuthorizationCode,
    OrderStatus Status,
    OrderStatusDetail StatusDetail);
