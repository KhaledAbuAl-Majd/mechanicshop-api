using MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.AssignLabor;

public class AssignLaborCommandValidatorTests
{
    private AssginLaborCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldFail_WhenWorkOrderIdEmpty()
    {
        var ct = CancellationToken.None;

        var command = new AssignLaborCommand(Guid.Empty, Guid.NewGuid());

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, result.Errors[0].ErrorCode);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenLaborIdEmpty()
    {
        var ct = CancellationToken.None;

        var command = new AssignLaborCommand(Guid.NewGuid(),Guid.Empty);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(WorkOrderErrors.LaborIdRequired.Code, result.Errors[0].ErrorCode);
    }

    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var command = new AssignLaborCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }
}
