using AIChallenge.Data;
using AIChallenge.Models;
using AIChallenge.Repositories;
using AIChallenge.Services;
using Xunit;

namespace AIChallenge.Tests;

public sealed class EcommerceServiceAdditionalTests
{
    [Fact]
    public async Task RegisterCustomerAsync_CreatesCustomer_WhenRequestIsValid()
    {
        AppData data = SeedData.Create();
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        RegisterCustomerRequest request = new(
            "  Ana Lopez  ",
            "gode561231hdfbcd05",
            new DateOnly(1990, 5, 20),
            new Address("Calle 1", "Roma Norte", "06700", "Cuauhtémoc", "Ciudad de México"));

        ApiResult<Customer> result = await service.RegisterCustomerAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("ANA LOPEZ", result.Data.FullName.ToUpperInvariant());
        Assert.Equal("GODE561231HDFBCD05", result.Data.Curp);
        Assert.Single(data.Customers);
    }

    [Fact]
    public async Task RegisterCustomerAsync_ReturnsInvalidCurp_WhenCurpIsMalformed()
    {
        AppData data = SeedData.Create();
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        RegisterCustomerRequest request = new(
            "Ana Lopez",
            "INVALID",
            new DateOnly(1990, 5, 20),
            new Address("Calle 1", "Roma Norte", "06700", "Cuauhtemoc", "Ciudad de Mexico"));

        ApiResult<Customer> result = await service.RegisterCustomerAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.InvalidCurp, result.Error?.Code);
    }

    [Fact]
    public async Task RegisterCustomerAsync_ReturnsUnderAge_WhenCustomerIsMinor()
    {
        AppData data = SeedData.Create();
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        RegisterCustomerRequest request = new(
            "Ana Lopez",
            "GODE561231HDFBCD05",
            new DateOnly(2010, 1, 1),
            new Address("Calle 1", "Roma Norte", "06700", "Cuauhtemoc", "Ciudad de Mexico"));

        ApiResult<Customer> result = await service.RegisterCustomerAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.UnderAge, result.Error?.Code);
    }

    [Fact]
    public async Task RegisterCustomerAsync_ReturnsInvalidAddress_WhenAddressIsUnknown()
    {
        AppData data = SeedData.Create();
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        RegisterCustomerRequest request = new(
            "Ana Lopez",
            "GODE561231HDFBCD05",
            new DateOnly(1990, 5, 20),
            new Address("Calle 1", "Desconocida", "99999", "X", "Y"));

        ApiResult<Customer> result = await service.RegisterCustomerAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.InvalidAddress, result.Error?.Code);
    }

    [Fact]
    public async Task RegisterPaymentMethodAsync_ReturnsCustomerNotFound_WhenCustomerDoesNotExist()
    {
        AppData data = SeedData.Create();
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        RegisterPaymentMethodRequest request = new(
            "CUS-UNKNOWN",
            "4111111111111111",
            CardType.Visa,
            "ANA LOPEZ",
            "12/30",
            "123");

        ApiResult<PaymentMethod> result = await service.RegisterPaymentMethodAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.CustomerNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task RegisterPaymentMethodAsync_ReturnsInvalidCardNumber_WhenCardFailsValidation()
    {
        AppData data = SeedData.Create();
        Customer customer = CreateCustomer();
        data.Customers.Add(customer);
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        RegisterPaymentMethodRequest request = new(
            customer.Id,
            "123456",
            CardType.Visa,
            "ANA LOPEZ",
            "12/30",
            "123");

        ApiResult<PaymentMethod> result = await service.RegisterPaymentMethodAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.InvalidCardNumber, result.Error?.Code);
    }

    [Fact]
    public async Task RegisterPaymentMethodAsync_ReturnsInvalidCardBrand_WhenCardTypeDoesNotMatch()
    {
        AppData data = SeedData.Create();
        Customer customer = CreateCustomer();
        data.Customers.Add(customer);
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        RegisterPaymentMethodRequest request = new(
            customer.Id,
            "4111111111111111",
            CardType.Mastercard,
            "ANA LOPEZ",
            "12/30",
            "123");

        ApiResult<PaymentMethod> result = await service.RegisterPaymentMethodAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.InvalidCardBrand, result.Error?.Code);
    }

    [Fact]
    public async Task RegisterPaymentMethodAsync_ReturnsInvalidExpiration_WhenFormatIsInvalid()
    {
        AppData data = SeedData.Create();
        Customer customer = CreateCustomer();
        data.Customers.Add(customer);
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        RegisterPaymentMethodRequest request = new(
            customer.Id,
            "4111111111111111",
            CardType.Visa,
            "ANA LOPEZ",
            "99/99",
            "123");

        ApiResult<PaymentMethod> result = await service.RegisterPaymentMethodAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.InvalidExpiration, result.Error?.Code);
    }

    [Fact]
    public async Task RegisterPaymentMethodAsync_ReturnsInvalidCvv_WhenCvvDoesNotMatchBrand()
    {
        AppData data = SeedData.Create();
        Customer customer = CreateCustomer();
        data.Customers.Add(customer);
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        RegisterPaymentMethodRequest request = new(
            customer.Id,
            "4111111111111111",
            CardType.Visa,
            "ANA LOPEZ",
            "12/30",
            "12");

        ApiResult<PaymentMethod> result = await service.RegisterPaymentMethodAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.InvalidCvv, result.Error?.Code);
    }

    [Fact]
    public async Task RegisterPaymentMethodAsync_ReturnsDuplicatePaymentMethod_WhenFingerprintExists()
    {
        AppData data = SeedData.Create();
        Customer customer = CreateCustomer();
        data.Customers.Add(customer);
        data.PaymentMethods.Add(CreatePaymentMethod(customer.Id));
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        RegisterPaymentMethodRequest request = new(
            customer.Id,
            "4111111111111111",
            CardType.Visa,
            "ANA LOPEZ",
            "12/30",
            "123");

        ApiResult<PaymentMethod> result = await service.RegisterPaymentMethodAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.DuplicatePaymentMethod, result.Error?.Code);
    }

    [Fact]
    public async Task RegisterPaymentMethodAsync_CreatesPaymentMethod_WhenRequestIsValid()
    {
        AppData data = SeedData.Create();
        Customer customer = CreateCustomer();
        data.Customers.Add(customer);
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        RegisterPaymentMethodRequest request = new(
            customer.Id,
            "4111111111111111",
            CardType.Visa,
            "  ANA LOPEZ  ",
            "12/30",
            "123");

        ApiResult<PaymentMethod> result = await service.RegisterPaymentMethodAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("**** **** **** 1111", result.Data.MaskedCardNumber);
        Assert.Equal("ANA LOPEZ", result.Data.CardholderName);
        Assert.Single(data.PaymentMethods);
    }

    [Fact]
    public async Task RegisterPaymentMethodAsync_CreatesPaymentMethod_WhenExistingFingerprintBelongsToAnotherCard()
    {
        AppData data = SeedData.Create();
        Customer customer = CreateCustomer();
        data.Customers.Add(customer);
        data.PaymentMethods.Add(new PaymentMethod(
            "PAY-OTHER",
            customer.Id,
            "**** **** **** 0004",
            Validators.FingerprintCardNumber("5105105105105100"),
            CardType.Mastercard,
            "OTHER",
            "12/30",
            DateTimeOffset.UtcNow));
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());

        ApiResult<PaymentMethod> result = await service.RegisterPaymentMethodAsync(new RegisterPaymentMethodRequest(
            customer.Id,
            "4111111111111111",
            CardType.Visa,
            "ANA LOPEZ",
            "12/30",
            "123"));

        Assert.True(result.Success);
        Assert.Equal(2, data.PaymentMethods.Count);
    }

    [Fact]
    public async Task ListProductsAsync_ReturnsProductsFromRepository()
    {
        AppData data = SeedData.Create();
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());

        IReadOnlyList<Product> products = await service.ListProductsAsync();

        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task CreateOrderAsync_ReturnsCustomerNotFound_WhenCustomerDoesNotExist()
    {
        AppData data = SeedData.Create();
        CapturingPurchaseLogger logger = new();
        EcommerceService service = CreateService(data, logger);
        CreateOrderRequest request = new(
            "CUS-MISSING",
            "PAY-MISSING",
            [new CreateOrderItemRequest("SKU-MOU-002", 1)]);

        ApiResult<PurchaseOrder> result = await service.CreateOrderAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.CustomerNotFound, result.Error?.Code);
        Assert.Single(logger.Logs);
        Assert.Equal(0m, logger.Logs[0].Total);
    }

    [Fact]
    public async Task CreateOrderAsync_ReturnsPaymentMethodNotFound_WhenPaymentIsMissingForCustomer()
    {
        AppData data = SeedData.Create();
        Customer customer = CreateCustomer();
        data.Customers.Add(customer);
        CapturingPurchaseLogger logger = new();
        EcommerceService service = CreateService(data, logger);
        CreateOrderRequest request = new(
            customer.Id,
            "PAY-MISSING",
            [new CreateOrderItemRequest("SKU-MOU-002", 1)]);

        ApiResult<PurchaseOrder> result = await service.CreateOrderAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.PaymentMethodNotFound, result.Error?.Code);
        Assert.Single(logger.Logs);
    }

    [Fact]
    public async Task CreateOrderAsync_ReturnsPaymentMethodNotFound_WhenPaymentIdExistsForAnotherCustomer()
    {
        AppData data = SeedData.Create();
        Customer customerA = CreateCustomer();
        Customer customerB = new(
            "CUS-TEST00000002",
            "Maria Lopez",
            "BADD110313HMCLNS09",
            new DateOnly(1991, 3, 13),
            new Address("Calle 2", "Roma Norte", "06700", "Cuauhtémoc", "Ciudad de México"),
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        data.Customers.Add(customerA);
        data.Customers.Add(customerB);
        data.PaymentMethods.Add(CreatePaymentMethod(customerB.Id));
        CapturingPurchaseLogger logger = new();
        EcommerceService service = CreateService(data, logger);
        CreateOrderRequest request = new(
            customerA.Id,
            "PAY-TEST00000001",
            [new CreateOrderItemRequest("SKU-MOU-002", 1)]);

        ApiResult<PurchaseOrder> result = await service.CreateOrderAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.PaymentMethodNotFound, result.Error?.Code);
        Assert.Single(logger.Logs);
    }

    [Fact]
    public async Task CreateOrderAsync_ReturnsInvalidQuantity_WhenItemsAreEmpty()
    {
        AppData data = SeedData.Create();
        Customer customer = CreateCustomer();
        PaymentMethod paymentMethod = CreatePaymentMethod(customer.Id);
        data.Customers.Add(customer);
        data.PaymentMethods.Add(paymentMethod);
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        CreateOrderRequest request = new(customer.Id, paymentMethod.Id, []);

        ApiResult<PurchaseOrder> result = await service.CreateOrderAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.InvalidQuantity, result.Error?.Code);
    }

    [Fact]
    public async Task CreateOrderAsync_ReturnsProductNotFound_WhenSkuDoesNotExist()
    {
        AppData data = SeedData.Create();
        Customer customer = CreateCustomer();
        PaymentMethod paymentMethod = CreatePaymentMethod(customer.Id);
        data.Customers.Add(customer);
        data.PaymentMethods.Add(paymentMethod);
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        CreateOrderRequest request = new(
            customer.Id,
            paymentMethod.Id,
            [new CreateOrderItemRequest("SKU-NOT-FOUND", 1)]);

        ApiResult<PurchaseOrder> result = await service.CreateOrderAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.ProductNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task CreateOrderAsync_GroupsDuplicateSkus_WhenOrderIsAccepted()
    {
        AppData data = SeedData.Create();
        Customer customer = CreateCustomer();
        PaymentMethod paymentMethod = CreatePaymentMethod(customer.Id);
        data.Customers.Add(customer);
        data.PaymentMethods.Add(paymentMethod);
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());
        CreateOrderRequest request = new(
            customer.Id,
            paymentMethod.Id,
            [
                new CreateOrderItemRequest("SKU-MOU-002", 1),
                new CreateOrderItemRequest("sku-mou-002", 2)
            ]);

        ApiResult<PurchaseOrder> result = await service.CreateOrderAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Products);
        Assert.Equal(3, result.Data.Products[0].Quantity);
        Assert.Equal(1047.00m, result.Data.Total);
    }

    [Fact]
    public async Task GetOrderAsync_ReturnsOrderNotFound_WhenOrderDoesNotExist()
    {
        AppData data = SeedData.Create();
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());

        ApiResult<PurchaseOrder> result = await service.GetOrderAsync("ORD-MISSING");

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.OrderNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task GetOrderAsync_ReturnsOrder_WhenOrderExistsIgnoringCase()
    {
        AppData data = SeedData.Create();
        PurchaseOrder order = new(
            "ORD-ABC123",
            "CUS-1",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            100m,
            [],
            "SIM-100000",
            OrderStatus.Accepted,
            new OrderStatusDetail(null, []));
        data.Orders.Add(order);
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());

        ApiResult<PurchaseOrder> result = await service.GetOrderAsync("ord-abc123");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(order.Id, result.Data.Id);
    }

    [Fact]
    public async Task ListCustomerOrdersAsync_ReturnsOrdersSortedByDateDesc_AndCaseInsensitiveCustomer()
    {
        AppData data = SeedData.Create();
        data.Orders.Add(new PurchaseOrder(
            "ORD-1",
            "CUS-ONE",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            100m,
            [],
            "SIM-100000",
            OrderStatus.Accepted,
            new OrderStatusDetail(null, [])));
        data.Orders.Add(new PurchaseOrder(
            "ORD-2",
            "cus-one",
            new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
            200m,
            [],
            "SIM-200000",
            OrderStatus.Accepted,
            new OrderStatusDetail(null, [])));
        data.Orders.Add(new PurchaseOrder(
            "ORD-3",
            "CUS-OTHER",
            new DateTimeOffset(2026, 1, 3, 0, 0, 0, TimeSpan.Zero),
            300m,
            [],
            "SIM-300000",
            OrderStatus.Accepted,
            new OrderStatusDetail(null, [])));
        EcommerceService service = CreateService(data, new CapturingPurchaseLogger());

        IReadOnlyList<PurchaseOrder> result = await service.ListCustomerOrdersAsync("cus-one");

        Assert.Equal(2, result.Count);
        Assert.Equal("ORD-2", result[0].Id);
        Assert.Equal("ORD-1", result[1].Id);
    }

    private static EcommerceService CreateService(AppData data, CapturingPurchaseLogger logger)
    {
        InMemoryEcommerceRepository repository = new(data);
        FixedTimeProvider timeProvider = new(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        return new EcommerceService(repository, logger, timeProvider);
    }

    private static Customer CreateCustomer()
    {
        Address address = new("Av. Insurgentes 123", "Roma Norte", "06700", "Cuauhtemoc", "Ciudad de Mexico");
        return new Customer(
            "CUS-TEST00000001",
            "Juan Perez Lopez",
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