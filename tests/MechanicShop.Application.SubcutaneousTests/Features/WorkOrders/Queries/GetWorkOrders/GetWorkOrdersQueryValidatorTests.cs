using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrders;

public class GetWorkOrdersQueryValidatorTests
{
    private readonly GetWorkOrdersQueryValidator _validator = new();

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task Validate_ShouldFail_WhenPageNotValid(int page)
    {
        var ct = CancellationToken.None;

        var command = new GetWorkOrdersQuery(Page: page, PageSize: 10, SearchTerm: string.Empty);

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

        var command = new GetWorkOrdersQuery(Page: 1, PageSize: pageSize, SearchTerm: string.Empty);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal("Page.Size.Invalid", result.Errors[0].ErrorCode);
    }

    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var command = new GetWorkOrdersQuery(Page: 1, PageSize: 10, SearchTerm: string.Empty);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }
}
