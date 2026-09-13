using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrderById;

public class GetWorkOrderByIdQueryValidatorTests
{
    private readonly GetWorkOrderByIdQueryValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldFail_WhenWorkOrderIdEmpty()
    {
        var ct = CancellationToken.None;

        var command = new GetWorkOrderByIdQuery(Guid.Empty);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, result.Errors[0].ErrorCode);
    }


    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var command = new GetWorkOrderByIdQuery(Guid.NewGuid());

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }
}
