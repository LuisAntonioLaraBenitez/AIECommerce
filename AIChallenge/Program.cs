using AIChallenge.Repositories;
using AIChallenge.Services;
using Microsoft.OpenApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
builder.Services.AddSingleton<IEcommerceRepository, JsonEcommerceRepository>();
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

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

public partial class Program;
