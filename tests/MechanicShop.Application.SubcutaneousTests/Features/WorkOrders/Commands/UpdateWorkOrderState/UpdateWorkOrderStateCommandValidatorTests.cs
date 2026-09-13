using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderState;

public class UpdateWorkOrderStateCommandValidatorTests
{
    private readonly UpdateWorkOrderStateCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldFail_WhenWorkOrderIdEmpty()
    {
        var ct = CancellationToken.None;

        var command = new UpdateWorkOrderStateCommand(Guid.Empty, WorkOrderState.InProgress);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, result.Errors[0].ErrorCode);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenStateInvalid()
    {
        var ct = CancellationToken.None;

        var state = (WorkOrderState)9999;

        var command = new UpdateWorkOrderStateCommand(Guid.NewGuid(), state);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
    }


    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var state = WorkOrderState.InProgress;

        var command = new UpdateWorkOrderStateCommand(Guid.NewGuid(), state);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }
}
