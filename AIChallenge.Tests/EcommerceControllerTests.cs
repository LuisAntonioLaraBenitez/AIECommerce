using AIChallenge.Controllers;
using AIChallenge.Models;
using AIChallenge.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace AIChallenge.Tests;

public sealed class EcommerceControllerTests
{
    [Fact]
    public async Task RegisterCustomerAsync_ReturnsCreatedOnSuccess()
    {
        FakeEcommerceService service = new();
        Customer customer = new(
            "CUS-1",
            "Ana",
            "GODE561231HDFBCD05",
            new DateOnly(1990, 5, 20),
            new Address("Calle", "Roma Norte", "06700", "Cuauhtemoc", "Ciudad de Mexico"),
            DateTimeOffset.UtcNow);
        service.RegisterCustomerResult = ApiResult<Customer>.Ok(customer);
        EcommerceController controller = new(service);

        IActionResult result = await controller.RegisterCustomerAsync(
            new RegisterCustomerRequest("Ana", customer.Curp, customer.BirthDate, customer.Address),
            CancellationToken.None);

        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
    }

    [Fact]
    public async Task RegisterCustomerAsync_ReturnsBadRequestOnDefaultErrorCode()
    {
        FakeEcommerceService service = new()
        {
            RegisterCustomerResult = ApiResult<Customer>.Fail(ErrorCodes.InvalidCurp, "bad")
        };
        EcommerceController controller = new(service);

        IActionResult result = await controller.RegisterCustomerAsync(
            new RegisterCustomerRequest("Ana", "BAD", new DateOnly(1990, 1, 1), new Address("Calle", "Roma Norte", "06700", "Cuauhtemoc", "Ciudad de Mexico")),
            CancellationToken.None);

        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public async Task RegisterPaymentMethodAsync_ReturnsCreatedOnSuccess()
    {
        FakeEcommerceService service = new();
        PaymentMethod payment = new("PAY-1", "CUS-1", "**** **** **** 1111", "F", CardType.Visa, "ANA", "12/30", DateTimeOffset.UtcNow);
        service.RegisterPaymentMethodResult = ApiResult<PaymentMethod>.Ok(payment);
        EcommerceController controller = new(service);

        IActionResult result = await controller.RegisterPaymentMethodAsync(
            new RegisterPaymentMethodRequest("CUS-1", "4111111111111111", CardType.Visa, "ANA", "12/30", "123"),
            CancellationToken.None);

        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
    }

    [Fact]
    public async Task ListProductsAsync_ReturnsOk()
    {
        FakeEcommerceService service = new()
        {
            Products = [new Product("SKU-1", "P", 10m, [])]
        };
        EcommerceController controller = new(service);

        IActionResult result = await controller.ListProductsAsync(CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
    }

    [Fact]
    public async Task CreateOrderAsync_ReturnsCreatedOnSuccess()
    {
        FakeEcommerceService service = new();
        PurchaseOrder order = new("ORD-1", "CUS-1", DateTimeOffset.UtcNow, 10m, [], "SIM-123456", OrderStatus.Accepted, new OrderStatusDetail(null, []));
        service.CreateOrderResult = ApiResult<PurchaseOrder>.Ok(order);
        EcommerceController controller = new(service);

        IActionResult result = await controller.CreateOrderAsync(new CreateOrderRequest("CUS-1", "PAY-1", []), CancellationToken.None);

        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
    }

    [Theory]
    [InlineData(ErrorCodes.CustomerNotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorCodes.PaymentMethodNotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorCodes.ProductNotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorCodes.OrderNotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorCodes.OrderLimitExceeded, StatusCodes.Status422UnprocessableEntity)]
    [InlineData(ErrorCodes.InvalidQuantity, StatusCodes.Status400BadRequest)]
    public async Task GetOrderAsync_MapsErrorCodesToExpectedStatus(string errorCode, int expectedStatus)
    {
        FakeEcommerceService service = new()
        {
            GetOrderResult = ApiResult<PurchaseOrder>.Fail(errorCode, "error")
        };
        EcommerceController controller = new(service);

        IActionResult result = await controller.GetOrderAsync("ORD-1", CancellationToken.None);

        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(expectedStatus, objectResult.StatusCode);
    }

    [Fact]
    public async Task ListCustomerOrdersAsync_ReturnsOk()
    {
        FakeEcommerceService service = new()
        {
            CustomerOrders =
            [
                new PurchaseOrder("ORD-1", "CUS-1", DateTimeOffset.UtcNow, 10m, [], "SIM-1", OrderStatus.Accepted, new OrderStatusDetail(null, []))
            ]
        };
        EcommerceController controller = new(service);

        IActionResult result = await controller.ListCustomerOrdersAsync("CUS-1", CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
    }
}

internal sealed class FakeEcommerceService : IEcommerceService
{
    public ApiResult<Customer> RegisterCustomerResult { get; set; } = ApiResult<Customer>.Fail(ErrorCodes.InvalidCurp, "error");

    public ApiResult<PaymentMethod> RegisterPaymentMethodResult { get; set; } = ApiResult<PaymentMethod>.Fail(ErrorCodes.InvalidCardNumber, "error");

    public ApiResult<PurchaseOrder> CreateOrderResult { get; set; } = ApiResult<PurchaseOrder>.Fail(ErrorCodes.InvalidQuantity, "error");

    public ApiResult<PurchaseOrder> GetOrderResult { get; set; } = ApiResult<PurchaseOrder>.Fail(ErrorCodes.OrderNotFound, "error");

    public IReadOnlyList<Product> Products { get; set; } = [];

    public IReadOnlyList<Address> AddressesByPostalCode { get; set; } = [];

    public IReadOnlyList<PurchaseOrder> CustomerOrders { get; set; } = [];

    public Task<ApiResult<Customer>> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(RegisterCustomerResult);
    }

    public Task<ApiResult<PaymentMethod>> RegisterPaymentMethodAsync(RegisterPaymentMethodRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(RegisterPaymentMethodResult);
    }

    public Task<IReadOnlyList<Product>> ListProductsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Products);
    }

    public Task<IReadOnlyList<Address>> ListAddressesByPostalCodeAsync(string postalCode, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AddressesByPostalCode);
    }

    public Task<ApiResult<PurchaseOrder>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateOrderResult);
    }

    public Task<ApiResult<PurchaseOrder>> GetOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetOrderResult);
    }

    public Task<IReadOnlyList<PurchaseOrder>> ListCustomerOrdersAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CustomerOrders);
    }
}
