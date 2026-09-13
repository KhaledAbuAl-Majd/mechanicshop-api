using MechanicShop.Application.Features.Customers.Queries.GetCustomers;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomers;

public class GetCustomersQueryValidatorTests
{

    private readonly GetCustomersQueryValidator _validator = new();

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task Validate_ShouldFail_WhenPageNotValid(int page)
    {
        var ct = CancellationToken.None;

        var command = new GetCustomersQuery(Page: page, PageSize: 10);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal("Page.Number.Invalid", result.Errors[0].ErrorCode);
    }


    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Validate_ShouldFail_WhenPageSizeNotValid(int pageSize)
    {
        var ct = CancellationToken.None;

        var command = new GetCustomersQuery(Page: 1, PageSize: pageSize);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal("Page.Size.Invalid", result.Errors[0].ErrorCode);
    }

    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var command = new GetCustomersQuery(Page: 1, PageSize: 10);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }
}
