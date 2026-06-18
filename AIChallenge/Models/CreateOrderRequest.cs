namespace AIChallenge.Models;

public sealed record CreateOrderRequest(
    string CustomerId,
    string PaymentMethodId,
    IReadOnlyList<CreateOrderItemRequest> Products);
