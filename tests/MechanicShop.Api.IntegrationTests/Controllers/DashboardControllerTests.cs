using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Api.IntegrationTests.Common.EndpointsPath;
using MechanicShop.Application.Features.Dashboard.Dtos;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class DashboardControllerTests : IAsyncLifetime
{
    private readonly AppHttpClient _client;
    private readonly WebAppFactory _factory;

    public DashboardControllerTests(WebAppFactory factory)
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
    public async Task GetWorkOrderStats_ShouldReturnTodayWorkOrderStatsDto_WhenValidData()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var date = DateOnly.FromDateTime(_factory.FakeTimeProvider.GetUtcNow().DateTime);
        var requestUri = $"{DashboardEndpointsPath.GetWorkOrderStats}?date={date:yyyy-MM-dd}";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        var tz = _factory.FakeTimeProvider.LocalTimeZone;
        request.Headers.Add("X-TimeZone", tz.Id);

        var response = await _client.SendAsync(request, ct: ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await _client.ReadFromJsonAsync<TodayWorkOrderStatsDto>(response, ct);

        Assert.NotNull(dto);
        Assert.Equal(date, dto.Date);
    }

    [Fact]
    public async Task GetWorkOrderStats_ShouldReturnBadRequest_WhenTimeZoneMissing()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var date = DateOnly.FromDateTime(_factory.FakeTimeProvider.GetUtcNow().DateTime);
        var requestUri = $"{DashboardEndpointsPath.GetWorkOrderStats}?date={date:yyyy-MM-dd}";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        var response = await _client.SendAsync(request, ct: ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkOrderStats_ShouldReturnBadRequest_WhenTimeZoneInvalid()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var date = DateOnly.FromDateTime(_factory.FakeTimeProvider.GetUtcNow().DateTime);
        var requestUri = $"{DashboardEndpointsPath.GetWorkOrderStats}?date={date:yyyy-MM-dd}";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        request.Headers.Add("X-TimeZone", "test");

        var response = await _client.SendAsync(request, ct: ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task GetWorkOrderStats_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);
        var requestUri = DashboardEndpointsPath.GetWorkOrderStats;

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public static TheoryData<string?> GetInvalidAuthentication() => new TheoryData<string?>()
    {
        null,
        string.Empty,
        "invalid token"
    };
}
