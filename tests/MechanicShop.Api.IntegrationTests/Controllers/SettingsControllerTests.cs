using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Api.IntegrationTests.Common.EndpointsPath;
using MechanicShop.Api.Responses;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class SettingsControllerTests : IAsyncLifetime
{
    private readonly AppHttpClient _client;
    private readonly WebAppFactory _factory;

    public SettingsControllerTests(WebAppFactory factory)
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
    public async Task GetOperatingHours_ShouldSuccess()
    {
        var ct = CancellationToken.None;
        _client.ClearAuthorizationHeader();
        var requestUri = SettingsEndpointsPath.GetOperatingHours;

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await _client.ReadFromJsonAsync<OperatingHoursResponse>(response, ct);

        Assert.NotNull(result);
    }

}
