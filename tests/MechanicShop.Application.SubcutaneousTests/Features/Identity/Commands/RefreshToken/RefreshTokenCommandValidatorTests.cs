using MechanicShop.Application.Features.Identity.Commands.RefreshToken;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Commands.RefreshToken;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_ShouldFail_WhenRefreshTokenInvalid(string? refrshToken)
    {
        var ct = CancellationToken.None;

        var command = new RefreshTokenCommand(refrshToken!, "expired access token");

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_ShouldFail_WhenExpiredAccessTokenInvalid(string? expiredAccessToken)
    {
        var ct = CancellationToken.None;

        var command = new RefreshTokenCommand("refresh token", expiredAccessToken!);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal("ExpiredAccessToken.Required", result.Errors[0].ErrorCode);
    }



    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var command = new RefreshTokenCommand("refresh token", "expired access token");

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }

}
