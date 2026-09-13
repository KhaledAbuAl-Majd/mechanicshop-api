using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicShop.Domain.RepairTasks;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.RemoveRepairTask;

public class RemoveRepairTaskCommandValidatorTests
{
    private readonly RemoveRepairTaskCommandValidator _validator = new();


    [Fact]
    public async Task Validate_ShouldFail_WhenIdInvalid()
    {
        var ct = CancellationToken.None;

        var command = new RemoveRepairTaskCommand(Guid.Empty);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(RepairTaskErrors.IdRequired.Code, result.Errors[0].ErrorCode);
    }

    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var command = new RemoveRepairTaskCommand(Guid.NewGuid());

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }
}
