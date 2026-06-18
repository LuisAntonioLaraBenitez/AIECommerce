using AIChallenge.Data;
using AIChallenge.Models;
using AIChallenge.Repositories;
using AIChallenge.Services;
using Xunit;

namespace AIChallenge.Tests;

public sealed class EcommerceServiceTests
{
    [Fact]
    public async Task RegisterCustomerAsync_ReturnsDuplicateError_WhenCurpExists()
    {
        AppData data = SeedData.Create();
        Customer existingCustomer = CreateCustomer();
        data.Customers.Add(existingCustomer);
        CapturingPurchaseLogger logger = new();
        EcommerceService service = CreateService(data, logger);
        RegisterCustomerRequest request = new(
            existingCustomer.FullName,
            existingCustomer.Curp,
            existingCustomer.BirthDate,
            existingCustomer.Address);

        ApiResult<Customer> result = await service.RegisterCustomerAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.DuplicateCustomer, result.Error?.Code);
    }

    [Fact]
    public async Task CreateOrderAsync_RejectsOrder_WhenTotalExceedsLimit()
    {
        AppData data = SeedData.Create();
        Customer customer = CreateCustomer();
        PaymentMethod paymentMethod = CreatePaymentMethod(customer.Id);
        data.Customers.Add(customer);
        data.PaymentMethods.Add(paymentMethod);
        CapturingPurchaseLogger logger = new();
        EcommerceService service = CreateService(data, logger);
        CreateOrderRequest request = new(
            customer.Id,
            paymentMethod.Id,
            [new CreateOrderItemRequest("SKU-LAP-001", 2)]);

        ApiResult<PurchaseOrder> result = await service.CreateOrderAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.OrderLimitExceeded, result.Error?.Code);
        Assert.Single(logger.Logs);
        PurchaseAttemptLog log = logger.Logs[0];
        Assert.False(log.Accepted);
        Assert.Equal(8598.00m, log.Total);
    }

    [Fact]
    public async Task CreateOrderAsync_CreatesAcceptedOrder_WhenRequestIsValid()
    {
        AppData data = SeedData.Create();
        Customer customer = CreateCustomer();
        PaymentMethod paymentMethod = CreatePaymentMethod(customer.Id);
        data.Customers.Add(customer);
        data.PaymentMethods.Add(paymentMethod);
        CapturingPurchaseLogger logger = new();
        EcommerceService service = CreateService(data, logger);
        CreateOrderRequest request = new(
            customer.Id,
            paymentMethod.Id,
            [new CreateOrderItemRequest("SKU-MOU-002", 2)]);

        ApiResult<PurchaseOrder> result = await service.CreateOrderAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        PurchaseOrder order = result.Data;
        Assert.Equal(OrderStatus.Accepted, order.Status);
        Assert.Equal(698.00m, order.Total);
        Assert.Single(logger.Logs);
        Assert.True(logger.Logs[0].Accepted);
    }

    private static EcommerceService CreateService(AppData data, CapturingPurchaseLogger logger)
    {
        InMemoryEcommerceRepository repository = new(data);
        FixedTimeProvider timeProvider = new(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        return new EcommerceService(repository, logger, timeProvider);
    }

    private static Customer CreateCustomer()
    {
        Address address = new("Av. Insurgentes 123", "Roma Norte", "06700", "Cuauhtémoc", "Ciudad de México");
        return new Customer(
            "CUS-TEST00000001",
            "Juan Pérez López",
            "GODE561231HDFBCD05",
            new DateOnly(1990, 5, 20),
            address,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }

    private static PaymentMethod CreatePaymentMethod(string customerId)
    {
        return new PaymentMethod(
            "PAY-TEST00000001",
            customerId,
            "**** **** **** 1111",
            Validators.FingerprintCardNumber("4111111111111111"),
            CardType.Visa,
            "JUAN PEREZ LOPEZ",
            "12/30",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }
}
