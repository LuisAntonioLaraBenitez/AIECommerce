namespace AIChallenge.Models;

public enum CardType
{
    Visa,
    Mastercard,
    Amex
}

public enum OrderStatus
{
    Accepted,
    Rejected,
    Preparing,
    Shipped,
    Delivered,
    Cancelled
}

public sealed record Address(
    string StreetAndNumber,
    string Neighborhood,
    string PostalCode,
    string Municipality,
    string State);

public sealed record Customer(
    string Id,
    string FullName,
    string Curp,
    DateOnly BirthDate,
    Address Address,
    DateTimeOffset CreatedAt);

public sealed record PaymentMethod(
    string Id,
    string CustomerId,
    string MaskedCardNumber,
    string CardFingerprint,
    CardType CardType,
    string CardholderName,
    string Expiration,
    DateTimeOffset CreatedAt);

public sealed record Product(
    string Sku,
    string Name,
    decimal Price,
    IReadOnlyList<string> Features);

public sealed record OrderItem(
    string Sku,
    string ProductName,
    decimal UnitPrice,
    int Quantity);

public sealed record OrderStatusDetail(
    string? RejectionReason,
    IReadOnlyList<string> DeliveryTracking);

public sealed record PurchaseOrder(
    string Id,
    string CustomerId,
    DateTimeOffset Date,
    decimal Total,
    IReadOnlyList<OrderItem> Products,
    string AuthorizationCode,
    OrderStatus Status,
    OrderStatusDetail StatusDetail);

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

public sealed class AppData
{
    public List<Customer> Customers { get; set; } = [];

    public List<PaymentMethod> PaymentMethods { get; set; } = [];

    public List<Product> Products { get; set; } = [];

    public List<Address> AddressCatalog { get; set; } = [];

    public List<PurchaseOrder> Orders { get; set; } = [];
}
