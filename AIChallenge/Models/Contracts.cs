namespace AIChallenge.Models;

public sealed record RegisterCustomerRequest(
    string FullName,
    string Curp,
    DateOnly BirthDate,
    Address Address);

public sealed record RegisterPaymentMethodRequest(
    string CustomerId,
    string CardNumber,
    CardType CardType,
    string CardholderName,
    string Expiration,
    string Cvv);

public sealed record CreateOrderItemRequest(
    string Sku,
    int Quantity);

public sealed record CreateOrderRequest(
    string CustomerId,
    string PaymentMethodId,
    IReadOnlyList<CreateOrderItemRequest> Products);

public sealed record ApiError(string Code, string Message);

public sealed record ApiResult<T>(bool Success, T? Data, ApiError? Error)
{
    public static ApiResult<T> Ok(T data) => new(true, data, null);

    public static ApiResult<T> Fail(string code, string message) => new(false, default, new ApiError(code, message));
}

public static class ErrorCodes
{
    public const string DuplicateCustomer = "CUSTOMER_DUPLICATE";
    public const string InvalidCurp = "CUSTOMER_CURP_INVALID";
    public const string UnderAge = "CUSTOMER_UNDER_AGE";
    public const string InvalidAddress = "ADDRESS_INVALID";
    public const string CustomerNotFound = "CUSTOMER_NOT_FOUND";
    public const string DuplicatePaymentMethod = "PAYMENT_DUPLICATE";
    public const string InvalidCardNumber = "PAYMENT_CARD_INVALID";
    public const string InvalidCardBrand = "PAYMENT_CARD_BRAND_MISMATCH";
    public const string InvalidExpiration = "PAYMENT_EXPIRATION_INVALID";
    public const string InvalidCvv = "PAYMENT_CVV_INVALID";
    public const string PaymentMethodNotFound = "PAYMENT_NOT_FOUND";
    public const string ProductNotFound = "PRODUCT_NOT_FOUND";
    public const string InvalidQuantity = "ORDER_QUANTITY_INVALID";
    public const string OrderLimitExceeded = "ORDER_LIMIT_EXCEEDED";
    public const string OrderNotFound = "ORDER_NOT_FOUND";
}
