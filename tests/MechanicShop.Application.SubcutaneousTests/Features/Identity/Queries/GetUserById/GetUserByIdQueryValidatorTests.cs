using MechanicShop.Application.Features.Identity.Queries.GetUserById;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.GetUserById;

public class GetUserByIdQueryValidatorTests
{
    private readonly GetUserByIdQueryValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldFail_WhenIdInvalid()
    {
        CancellationToken ct = CancellationToken.None;
        var query = new GetUserByIdQuery(string.Empty);

        var result = await _validator.ValidateAsync(query, ct);

        Assert.False(result.IsValid);
        Assert.Equal("User.Id.Required", result.Errors[0].ErrorCode);
    }


    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        CancellationToken ct = CancellationToken.None;
        var query = new GetUserByIdQuery(Guid.NewGuid().ToString());

        var result = await _validator.ValidateAsync(query, ct);

        Assert.True(result.IsValid);
    }
}
