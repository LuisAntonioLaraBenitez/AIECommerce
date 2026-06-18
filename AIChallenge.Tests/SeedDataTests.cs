using AIChallenge.Data;
using AIChallenge.Models;
using Xunit;

namespace AIChallenge.Tests;

public sealed class SeedDataTests
{
    [Fact]
    public void Create_InitializesProductsAndAddressCatalog()
    {
        AppData data = SeedData.Create();

        Assert.NotEmpty(data.Products);
        Assert.NotEmpty(data.AddressCatalog);
    }

    [Fact]
    public void EnsureProducts_AddsMissingProducts_AndReturnsTrue()
    {
        AppData data = new();

        bool changed = SeedData.EnsureProducts(data);

        Assert.True(changed);
        Assert.NotEmpty(data.Products);
    }

    [Fact]
    public void EnsureProducts_DoesNotDuplicateExistingProducts_AndReturnsFalse()
    {
        AppData data = SeedData.Create();

        bool changed = SeedData.EnsureProducts(data);

        Assert.False(changed);
    }

    [Fact]
    public void EnsureAddressCatalog_AddsMissingAddresses_AndReturnsTrue()
    {
        AppData data = new();

        bool changed = SeedData.EnsureAddressCatalog(data);

        Assert.True(changed);
        Assert.NotEmpty(data.AddressCatalog);
    }

    [Fact]
    public void EnsureAddressCatalog_DoesNotDuplicateExistingAddresses_AndReturnsFalse()
    {
        AppData data = SeedData.Create();

        bool changed = SeedData.EnsureAddressCatalog(data);

        Assert.False(changed);
    }
}
