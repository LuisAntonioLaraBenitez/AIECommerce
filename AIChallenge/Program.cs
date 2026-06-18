using AIChallenge.Data;
using AIChallenge.Models;
using AIChallenge.Services;
using Microsoft.OpenApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AIChallenge E-commerce API",
        Version = "v1",
        Description = "Local API for customer registration, payment method registration, marketplace purchases, and order tracking."
    });
    options.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = JsonSchemaType.String,
        Format = "date"
    });
});
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IDataStore, JsonDataStore>();
builder.Services.AddSingleton<IPurchaseLogger, PurchaseLogger>();
builder.Services.AddScoped<IEcommerceService, EcommerceService>();

WebApplication app = builder.Build();

app.UseSwagger(options =>
{
    options.RouteTemplate = "swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AIChallenge E-commerce API v1");
});

RouteGroupBuilder api = app.MapGroup("/api");

api.MapPost("/customers", async (RegisterCustomerRequest request, IEcommerceService service, CancellationToken cancellationToken) =>
{
    ApiResult<Customer> result = await service.RegisterCustomerAsync(request, cancellationToken);
    return ToHttpResult(result, StatusCodes.Status201Created);
})
.WithName("RegisterCustomer");

api.MapPost("/payment-methods", async (RegisterPaymentMethodRequest request, IEcommerceService service, CancellationToken cancellationToken) =>
{
    ApiResult<PaymentMethod> result = await service.RegisterPaymentMethodAsync(request, cancellationToken);
    return ToHttpResult(result, StatusCodes.Status201Created);
})
.WithName("RegisterPaymentMethod");

api.MapGet("/products", async (IEcommerceService service, CancellationToken cancellationToken) =>
{
    IReadOnlyList<Product> products = await service.ListProductsAsync(cancellationToken);
    return Results.Ok(products);
})
.WithName("ListProducts")
.Produces<IReadOnlyList<Product>>(StatusCodes.Status200OK);

api.MapPost("/orders", async (CreateOrderRequest request, IEcommerceService service, CancellationToken cancellationToken) =>
{
    ApiResult<PurchaseOrder> result = await service.CreateOrderAsync(request, cancellationToken);
    return ToHttpResult(result, StatusCodes.Status201Created);
})
.WithName("CreateOrder");

api.MapGet("/orders/{orderId}", async (string orderId, IEcommerceService service, CancellationToken cancellationToken) =>
{
    ApiResult<PurchaseOrder> result = await service.GetOrderAsync(orderId, cancellationToken);
    return ToHttpResult(result);
})
.WithName("GetOrder");

api.MapGet("/customers/{customerId}/orders", async (string customerId, IEcommerceService service, CancellationToken cancellationToken) =>
{
    IReadOnlyList<PurchaseOrder> orders = await service.ListCustomerOrdersAsync(customerId, cancellationToken);
    return Results.Ok(orders);
})
.WithName("ListCustomerOrders");

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

static IResult ToHttpResult<T>(ApiResult<T> result, int successStatusCode = StatusCodes.Status200OK)
{
    if (result.Success)
    {
        return Results.Json(result.Data, statusCode: successStatusCode);
    }

    int statusCode = result.Error?.Code switch
    {
        ErrorCodes.CustomerNotFound or ErrorCodes.PaymentMethodNotFound or ErrorCodes.ProductNotFound or ErrorCodes.OrderNotFound => StatusCodes.Status404NotFound,
        ErrorCodes.OrderLimitExceeded => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status400BadRequest
    };

    return Results.Json(result.Error, statusCode: statusCode);
}

public partial class Program;
