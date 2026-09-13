using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.DeleteWorkOrder;

public class DeleteWorkOrderCommandValidatorTests
{
    private readonly DeleteWorkOrderCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldFail_WhenWorkOrderIdEmpty()
    {
        var ct = CancellationToken.None;

        var command = new DeleteWorkOrderCommand(Guid.Empty);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, result.Errors[0].ErrorCode);
    }


    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var command = new DeleteWorkOrderCommand(Guid.NewGuid());

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }
}
