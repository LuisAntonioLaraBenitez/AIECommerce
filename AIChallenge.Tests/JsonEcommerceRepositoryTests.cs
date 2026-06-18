using System.Text.Json;
using AIChallenge.Models;
using AIChallenge.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace AIChallenge.Tests;

public sealed class JsonEcommerceRepositoryTests
{
    [Fact]
    public async Task ReadAsync_SeedsFile_WhenMissing()
    {
        using TestWebHostEnvironment env = new();
        JsonEcommerceRepository repository = new(env);

        AppData data = await repository.ReadAsync();

        Assert.NotEmpty(data.Products);
        Assert.NotEmpty(data.AddressCatalog);
    }

    [Fact]
    public async Task WriteAsync_PersistsData_AndReadAsyncRestoresIt()
    {
        using TestWebHostEnvironment env = new();
        JsonEcommerceRepository repository = new(env);
        AppData data = await repository.ReadAsync();
        Customer customer = new(
            "CUS-TEST",
            "Ana Lopez",
            "GODE561231HDFBCD05",
            new DateOnly(1990, 5, 20),
            new Address("Calle 1", "Roma Norte", "06700", "Cuauhtemoc", "Ciudad de Mexico"),
            DateTimeOffset.UtcNow);
        data.Customers.Add(customer);

        await repository.WriteAsync(data);
        AppData reloaded = await repository.ReadAsync();

        Assert.Contains(reloaded.Customers, x => x.Id == "CUS-TEST");
    }

    [Fact]
    public async Task ReadAsync_FixesInvalidDataByEnsuringSeededCollections()
    {
        using TestWebHostEnvironment env = new();
        string dataDir = Path.Combine(env.ContentRootPath, "DataStore");
        Directory.CreateDirectory(dataDir);
        string file = Path.Combine(dataDir, "ecommerce.json");
        AppData empty = new();
        string json = JsonSerializer.Serialize(empty, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await File.WriteAllTextAsync(file, json);

        JsonEcommerceRepository repository = new(env);
        AppData reloaded = await repository.ReadAsync();

        Assert.NotEmpty(reloaded.Products);
        Assert.NotEmpty(reloaded.AddressCatalog);
    }
}

internal sealed class TestWebHostEnvironment : IWebHostEnvironment, IDisposable
{
    public TestWebHostEnvironment()
    {
        ContentRootPath = Path.Combine(Path.GetTempPath(), "aichallenge-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ContentRootPath);
        EnvironmentName = "Development";
        ApplicationName = "AIChallenge.Tests";
        WebRootPath = Path.Combine(ContentRootPath, "wwwroot");
        ContentRootFileProvider = null!;
        WebRootFileProvider = null!;
    }

    public string EnvironmentName { get; set; }

    public string ApplicationName { get; set; }

    public string WebRootPath { get; set; }

    public IFileProvider WebRootFileProvider { get; set; }

    public string ContentRootPath { get; set; }

    public IFileProvider ContentRootFileProvider { get; set; }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(ContentRootPath))
            {
                Directory.Delete(ContentRootPath, recursive: true);
            }
        }
        catch
        {
        }
    }
}
