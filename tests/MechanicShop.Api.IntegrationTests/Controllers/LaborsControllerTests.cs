using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Api.IntegrationTests.Common.EndpointsPath;
using MechanicShop.Application.Features.Labors.Dtos;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class LaborsControllerTests : IAsyncLifetime
{
    private readonly AppHttpClient _client;
    private readonly WebAppFactory _factory;

    public LaborsControllerTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAppHttpClient();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }


    [Fact]
    public async Task GetLabors_ShouldReturnLaborList_WhenValidData()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);
        var requestUri = LaborEndpointsPath.GetLabors;

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await _client.ReadFromJsonAsync<List<LaborDto>>(response, ct);

        Assert.NotNull(result);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task GetLabors_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);
        var requestUri = LaborEndpointsPath.GetLabors;

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetLabors_ShouldReturnForbidden_WithoutManagerRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);
        var requestUri = LaborEndpointsPath.GetLabors;

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    public static TheoryData<string?> GetInvalidAuthentication() => new TheoryData<string?>()
    {
        null,
        string.Empty,
        "invalid token"
    };
}
