using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.RelocateWorkOrder;

public class RelocateWorkOrderCommandValidatorTests
{

    private readonly RelocateWorkOrderCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldFail_WhenWorkOrderIdEmpty()
    {
        var ct = CancellationToken.None;

        var command = new RelocateWorkOrderCommand(Guid.Empty, DateTime.UtcNow.AddHours(1), Spot.C);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, result.Errors[0].ErrorCode);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenStartAtNotInFuture()
    {
        var ct = CancellationToken.None;

        var command = new RelocateWorkOrderCommand(Guid.NewGuid(), DateTime.UtcNow.AddHours(-1), Spot.C);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenSpotInvalid()
    {
        var ct = CancellationToken.None;

        var spot = (Spot)999;

        var command = new RelocateWorkOrderCommand(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), spot);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == WorkOrderErrors.SpotInvalid.Code);
    }
}
