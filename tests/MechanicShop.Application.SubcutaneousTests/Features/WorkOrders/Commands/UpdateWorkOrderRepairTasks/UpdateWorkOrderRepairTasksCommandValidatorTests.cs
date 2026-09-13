using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public class UpdateWorkOrderRepairTasksCommandValidatorTests
{
    private readonly UpdateWorkOrderRepairTasksCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldFail_WhenWorkOrderIdEmpty()
    {
        var ct = CancellationToken.None;

        var command = new UpdateWorkOrderRepairTasksCommand(Guid.Empty, [Guid.NewGuid()]);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, result.Errors[0].ErrorCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidRepairTaskIds))]
    public async Task Validate_ShouldFail_WhenRepairTaskIdsInvalid(List<Guid>? repairTaksIds)
    {
        var ct = CancellationToken.None;

        Guid[]? ids = repairTaksIds is null ? null : [.. repairTaksIds];

        var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), ids!);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(RepairTaskErrors.AtLeastOneRepairTaskIsRequired.Code, result.Errors[0].ErrorCode);
    }

    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), [Guid.NewGuid()]);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }

    public static TheoryData<List<Guid>?> GetInvalidRepairTaskIds() => new TheoryData<List<Guid>?>()
    {
        null,
        new List<Guid>()
    };
}
