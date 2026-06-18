using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AIChallenge.Tests;

public sealed class ProgramIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProgramIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ApiInfo_ReturnsOkPayload()
    {
        HttpResponseMessage response = await _client.GetAsync("/api-info");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ApiInfoResponse? payload = await response.Content.ReadFromJsonAsync<ApiInfoResponse>();
        Assert.NotNull(payload);
        Assert.Equal("AIChallenge E-commerce API", payload.Name);
        Assert.Equal("v1", payload.Version);
        Assert.Equal("ok", payload.Status);
    }

    [Fact]
    public async Task SwaggerDocument_IsAvailable()
    {
        HttpResponseMessage response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProductsEndpoint_ReturnsOk()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed record ApiInfoResponse(string Name, string Version, string Status, DateTimeOffset Timestamp);
}
