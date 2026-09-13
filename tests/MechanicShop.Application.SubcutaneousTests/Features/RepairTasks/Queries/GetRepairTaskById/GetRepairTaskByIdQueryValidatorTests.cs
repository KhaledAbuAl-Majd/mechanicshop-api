using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using MechanicShop.Domain.RepairTasks;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTaskById;

public class GetRepairTaskByIdQueryValidatorTests
{
    private readonly GetRepairTaskByIdQueryValidator _validator = new();


    [Fact]
    public async Task Validate_ShouldFail_WhenIdInvalid()
    {
        var ct = CancellationToken.None;

        var query = new GetRepairTaskByIdQuery(Guid.Empty);

        var result = await _validator.ValidateAsync(query, ct);

        Assert.False(result.IsValid);
        Assert.Equal(RepairTaskErrors.IdRequired.Code, result.Errors[0].ErrorCode);
    }

    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var query = new GetRepairTaskByIdQuery(Guid.NewGuid());

        var result = await _validator.ValidateAsync(query, ct);

        Assert.True(result.IsValid);
    }
}
