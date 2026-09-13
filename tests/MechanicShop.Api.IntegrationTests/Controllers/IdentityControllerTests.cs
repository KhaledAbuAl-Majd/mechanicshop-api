using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Api.IntegrationTests.Common.EndpointsPath;
using MechanicShop.Api.Requests.V1.Identity;
using MechanicShop.Api.Responses.V1.Identity;
using MechanicShop.Application.Common.Settings;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Tests.Common.Security;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class IdentityControllerTests : IAsyncLifetime
{
    private readonly AppHttpClient _client;
    private readonly WebAppFactory _factory;

    private readonly JwtSettings _jwtSettings;

    public IdentityControllerTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAppHttpClient();
        _jwtSettings = factory.Services.GetRequiredService<JwtSettings>();
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
    public async Task GenerateToken_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var requestUri = IdentityEndpointsPath.GenerateToken;

        var user = TestUsers.Manager;
        var request = new GenerateTokenRequest(user.Email!, user.Email!);

        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: false, ct: ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await _client.ReadFromJsonAsync<TokenDto>(response, ct);
        Assert.NotNull(dto);
    }

    [Fact]
    public async Task GenerateToken_ShouldReturnUnauthorized_WhenCredentialsInvalid()
    {
        var ct = CancellationToken.None;

        var requestUri = IdentityEndpointsPath.GenerateToken;

        var user = TestUsers.Manager;
        var request = new GenerateTokenRequest(user.Email!, user.Email + "df");

        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: false, ct: ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_ShouldSuccess_WhenValidData()
    {
        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        var requestUri = IdentityEndpointsPath.RefreshToken;

        _factory.FakeTimeProvider.SetUtcNow(token.ExpiresOnUtc.AddMinutes(1));

        var request = new RefreshTokenRequest(
            RefreshToken: token.RefreshToken,
            ExpiredAccessToken: token.AccessToken);

        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: false, ct: ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await _client.ReadFromJsonAsync<TokenDto>(response, ct);
        Assert.NotNull(dto);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnBadRequest_WhenAccessTokenNotExpired()
    {
        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        var requestUri = IdentityEndpointsPath.RefreshToken;

        var request = new RefreshTokenRequest(
            RefreshToken: token.RefreshToken,
            ExpiredAccessToken: token.AccessToken);

        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: false, ct: ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnBadRequest_WhenRefreshTokenExpired()
    {
        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        var requestUri = IdentityEndpointsPath.RefreshToken;

        _factory.FakeTimeProvider.SetUtcNow(_factory.FakeTimeProvider.GetUtcNow().AddDays(_jwtSettings.RefreshTokenExpirationInDays).AddMinutes(1));

        var request = new RefreshTokenRequest(
            RefreshToken: token.RefreshToken,
            ExpiredAccessToken: token.AccessToken);

        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: false, ct: ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnBadRequest_WhenRefreshTokenNotExist()
    {
        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        var requestUri = IdentityEndpointsPath.RefreshToken;

        _factory.FakeTimeProvider.SetUtcNow(token.ExpiresOnUtc.AddMinutes(1));

        var request = new RefreshTokenRequest(
            RefreshToken: "refresh token",
            ExpiredAccessToken: token.AccessToken);

        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: false, ct: ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    [Fact]
    public async Task GetCurrentUserInfo_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;
        var user = TestUsers.Labor02;
        var token = await _client.GenerateTokenAsync(user, ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = IdentityEndpointsPath.GetCurrentUserInfo;

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await _client.ReadFromJsonAsync<AppUserResponse>(response, ct);
        Assert.NotNull(dto);
        Assert.Equal(user.Id, dto.UserId);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task GetCurrentUserInfo_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;

        _client.SetAuthorizationHeader(authentication!);

        var requestUri = IdentityEndpointsPath.GetCurrentUserInfo;

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var user = TestUsers.Labor02;
        var requestUri = IdentityEndpointsPath.GetUserById(Guid.Parse(user.Id));

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await _client.ReadFromJsonAsync<AppUserResponse>(response, ct);
        Assert.NotNull(dto);
        Assert.Equal(user.Id, dto.UserId);
    }


    [Fact]
    public async Task GetUserById_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = IdentityEndpointsPath.GetUserById(Guid.NewGuid());

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task GetUserById_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;

        _client.SetAuthorizationHeader(authentication!);

        var requestUri = IdentityEndpointsPath.GetUserById(Guid.NewGuid());

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_ShouldSuccess_WithUserSelf()
    {
        var ct = CancellationToken.None;
        var user = TestUsers.Labor02;
        var token = await _client.GenerateTokenAsync(user, ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = IdentityEndpointsPath.GetUserById(Guid.Parse(user.Id));

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await _client.ReadFromJsonAsync<AppUserResponse>(response, ct);
        Assert.NotNull(dto);
        Assert.Equal(user.Id, dto.UserId);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnForbidden_WithoutUserSelf()
    {
        var ct = CancellationToken.None;
        var loggedInUser = TestUsers.Labor02;
        var token = await _client.GenerateTokenAsync(loggedInUser, ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var searchForUser = TestUsers.Labor03;
        var requestUri = IdentityEndpointsPath.GetUserById(Guid.Parse(searchForUser.Id));

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
