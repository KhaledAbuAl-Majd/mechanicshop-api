using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using MechanicShop.Tests.Common;
using MechanicShop.Tests.Common.Identity;

namespace MechanicShop.Domain.UnitTests.Identity;

public class RefreshTokenTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenIdEmpty()
    {
        var result = RefreshTokenFactory.CreateRefreshToken(id: Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.IdRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void Create_ShouldReturnError_WhenTokenHashInvalid(string? tokenHash)
    {
        var result = RefreshTokenFactory.CreateRefreshToken(tokenHash: tokenHash);

        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.TokenRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void Create_ShouldReturnError_WhenUserIdInvalid(string? userId)
    {
        var result = RefreshTokenFactory.CreateRefreshToken(userId: userId);

        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.UserIdRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenExpiryInvalid()
    {
        var time = new FakeTimeProvider();
        time.SetUtcNow(DateTimeOffset.UtcNow);

        var expiry = time.GetUtcNow().AddMinutes(-1);

        var result = RefreshTokenFactory.CreateRefreshToken(expiresOnUtc: expiry, provider: time);

        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.ExpiryInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataValid()
    {
        var time = new FakeTimeProvider();
        time.SetUtcNow(DateTimeOffset.UtcNow);

        Guid id = Guid.NewGuid();
        string tokenHash = "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08";
        string userId = Guid.NewGuid().ToString();
        var expiry = time.GetUtcNow().AddDays(7);

        var result = RefreshTokenFactory.CreateRefreshToken(
            id: id,
            tokenHash: tokenHash,
            userId: userId,
            expiresOnUtc: expiry,
            provider: time);


        Assert.True(result.IsSuccess);
        var token = result.Value;
        Assert.NotNull(token);
        Assert.Equal(id, token.Id);
        Assert.Equal(tokenHash, token.TokenHash);
        Assert.Equal(userId, token.UserId);
        Assert.Equal(expiry, token.ExpiresOnUtc);
        Assert.False(token.IsRevoked);
        Assert.Null(token.RevokedAt);
        Assert.True(token.IsActive(time));
    }


    [Fact]
    public void Revoke_ShouldReturnError_WhenAlreadyRevoked()
    {
        var time = new FakeTimeProvider();
        time.SetUtcNow(DateTimeOffset.UtcNow.AddDays(1));

        var token = RefreshTokenFactory.CreateRefreshToken().Value;

        token.Revoke(time);
        var result = token.Revoke(time);

        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.RefreshTokenAlreadyRevoked.Code, result.TopError.Code);
    }

    [Fact]
    public void Revoke_ShouldReturnSuccess_WhenValidData()
    {
        var time = new FakeTimeProvider();
        time.SetUtcNow(DateTimeOffset.UtcNow.AddDays(1));

        var token = RefreshTokenFactory.CreateRefreshToken().Value;

        var result = token.Revoke(time);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
        Assert.True(token.IsRevoked);
        Assert.Equal(time.GetUtcNow(), token.RevokedAt);
    }
}
