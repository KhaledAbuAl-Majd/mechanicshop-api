using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Settings;
using MechanicShop.Application.Common.Utilities;
using MechanicShop.Application.Features.Identity.Commands.RefreshToken;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.Identity.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Commands.RefreshToken;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RefreshTokenCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    private readonly JwtSettings _jwtSettings;

    public RefreshTokenCommandHandlerTests(WebAppFactory factory)
    {
        _factory = factory;
        (_mediator, _context, _scope) = factory.CreateMediatorAndAppDbContext();
        _jwtSettings = factory.Services.GetRequiredService<JwtSettings>();
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }


    [Fact]
    public async Task Handle_ShouldFail_WhenAccessTokenInvalid()
    {
        var ct = CancellationToken.None;

        var command = new RefreshTokenCommand("refresh token", "invalid access token");

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.ExpiredAccessTokenInvalid.Code, result.TopError.Code);
    }



    [Fact]
    public async Task Handle_ShouldFail_WhenAccessTokenNotExpired()
    {
        var ct = CancellationToken.None;
        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

        var tokenDto = await IdentityTestHelper.GenerateValidManagerToken(_mediator, ct);

        var command = new RefreshTokenCommand(tokenDto.RefreshToken, tokenDto.AccessToken);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.AccessTokenNotExpired.Code, result.TopError.Code);
    }


    [Fact]
    public async Task Handle_ShouldFail_WhenRefreshTokenInvalid()
    {
        var ct = CancellationToken.None;
        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

        var tokenDto = await IdentityTestHelper.GenerateValidManagerToken(_mediator, ct);

        //access token already expired
        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes + 1));

        var command = new RefreshTokenCommand("unknown refresh token", tokenDto.AccessToken);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RefreshTokenInvalidOrExpired.Code, result.TopError.Code);
    }


    [Fact]
    public async Task Handle_ShouldFail_WhenRefreshTokenExpired()
    {
        var ct = CancellationToken.None;
        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

        var tokenDto = await IdentityTestHelper.GenerateValidManagerToken(_mediator, ct);

        //access token and refresh token already expired
        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays).AddMinutes(1));

        var command = new RefreshTokenCommand(tokenDto.RefreshToken, tokenDto.AccessToken);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RefreshTokenInvalidOrExpired.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRefreshTokenRevoked()
    {
        var ct = CancellationToken.None;
        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

        var oldTokenDto = await IdentityTestHelper.GenerateValidManagerToken(_mediator, ct);

        var oldRefreshTokenHashed = HashHelper.ComputeSha256(oldTokenDto.RefreshToken);
        var oldRefreshToken = await _context.RefreshTokens.SingleAsync(rt => rt.TokenHash == oldRefreshTokenHashed, ct);

        oldRefreshToken.Revoke(_factory.FakeTimeProvider);

        await _context.SaveChangesAsync(ct);

        //access token already expired
        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes + 1));

        var command = new RefreshTokenCommand(oldTokenDto.RefreshToken, oldTokenDto.AccessToken);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RefreshTokenInvalidOrExpired.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;
        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

        var oldTokenDto = await IdentityTestHelper.GenerateValidManagerToken(_mediator, ct);

        //access token already expired
        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes + 1));

        var command = new RefreshTokenCommand(oldTokenDto.RefreshToken, oldTokenDto.AccessToken);

        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        var generatedTokenDto = result.Value;
        Assert.NotNull(generatedTokenDto);
        Assert.Equal(_factory.FakeTimeProvider.GetUtcNow().AddMinutes(_jwtSettings.TokenExpirationInMinutes), generatedTokenDto.ExpiresOnUtc);

        var generatedRefreshTokenHashed = HashHelper.ComputeSha256(generatedTokenDto.RefreshToken);
        var generatedRefreshTokenExists = await _context.RefreshTokens.AnyAsync(rt => rt.TokenHash == generatedRefreshTokenHashed && !rt.IsRevoked, ct);
        Assert.True(generatedRefreshTokenExists);

        var oldRefreshTokenHashed = HashHelper.ComputeSha256(oldTokenDto.RefreshToken);
        var isOldRefreshTokenRevoked = await _context.RefreshTokens.AnyAsync(rt => rt.TokenHash == oldRefreshTokenHashed && rt.IsRevoked, ct);
        Assert.True(isOldRefreshTokenRevoked);
    }
}
