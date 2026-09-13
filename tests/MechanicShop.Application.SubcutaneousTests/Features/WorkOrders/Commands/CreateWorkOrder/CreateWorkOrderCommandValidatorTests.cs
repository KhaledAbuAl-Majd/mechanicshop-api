using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandValidatorTests
{
    private readonly CreateWorkOrderCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldFail_WhenVehicleIdEmpty()
    {
        var ct = CancellationToken.None;

        var command = new CreateWorkOrderCommand(
            Spot.A,
            Guid.Empty,
            DateTime.UtcNow.AddHours(1),
            [Guid.NewGuid()],
            Guid.NewGuid());

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == WorkOrderErrors.VehicleIdRequired.Code);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenStartAtNotInFuture()
    {
        var ct = CancellationToken.None;

        var command = new CreateWorkOrderCommand(
            Spot.A,
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(-1),
            [Guid.NewGuid()],
            Guid.NewGuid());

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
    }

    [Theory]
    [MemberData(nameof(GetInvalidRepairTaskIds))]
    public async Task Validate_ShouldFail_WhenRepairTaskIdsInvalid(List<Guid>? repairTaksIds)
    {
        var ct = CancellationToken.None;

        var command = new CreateWorkOrderCommand(
            Spot.A,
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1),
            repairTaksIds!,
            Guid.NewGuid());

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenLaborIdEmpty()
    {
        var ct = CancellationToken.None;

        var command = new CreateWorkOrderCommand(
            Spot.A,
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1),
            [Guid.NewGuid()],
             Guid.Empty);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == WorkOrderErrors.LaborIdRequired.Code);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenSpotInvalid()
    {
        var ct = CancellationToken.None;

        var spot = (Spot)999;

        var command = new CreateWorkOrderCommand(
            spot,
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1),
            [Guid.NewGuid()],
            Guid.NewGuid());

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == WorkOrderErrors.SpotInvalid.Code);
    }

    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var command = new CreateWorkOrderCommand(
            Spot.A,
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1),
            [Guid.NewGuid()],
            Guid.NewGuid());

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }

    public static TheoryData<List<Guid>?> GetInvalidRepairTaskIds() => new TheoryData<List<Guid>?>()
    {
        null,
        new List<Guid>()
    };

}
