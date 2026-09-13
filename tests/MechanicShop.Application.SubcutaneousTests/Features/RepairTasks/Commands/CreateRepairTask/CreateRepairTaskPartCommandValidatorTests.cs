using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.CreateRepairTask;

public class CreateRepairTaskPartCommandValidatorTests
{
    private readonly CreateRepairTaskPartCommandValidator _validator = new();


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_ShouldFail_WhenNameInvalid(string? name)
    {
        var ct = CancellationToken.None;

        var command = new CreateRepairTaskPartCommand(Name: name!, Cost: 40, Quantity: 3);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(PartErrors.NameRequired.Code, result.Errors[0].ErrorCode);
    }

    [Theory]
    [MemberData(nameof(InvalidCostData))]
    public async Task Validate_ShouldFail_WhenCostInvalid(decimal cost)
    {
        var ct = CancellationToken.None;

        var command = new CreateRepairTaskPartCommand(Name: "oilFilter-1234", Cost: cost, Quantity: 3);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(PartErrors.CostInvalid.Code, result.Errors[0].ErrorCode);
    }



    [Theory]
    [MemberData(nameof(InvalidQuantityData))]
    public async Task Validate_ShouldFail_WhenQuantityInvalid(int qunaity)
    {
        var ct = CancellationToken.None;

        var command = new CreateRepairTaskPartCommand(Name: "oilFilter-1234", Cost: 30, Quantity: qunaity);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(PartErrors.QuantityInvalid.Code, result.Errors[0].ErrorCode);
    }



    [Fact]

    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var command = new CreateRepairTaskPartCommand(Name: "oilFilter-1234", Cost: 30, Quantity: 2);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }




    public static TheoryData<decimal> InvalidCostData() => new TheoryData<decimal>()
    {
        PartConstant.ExclusiveMinCost,
        PartConstant.ExclusiveMinCost -1,
        PartConstant.MaxCost  + 1
    };
    public static TheoryData<int> InvalidQuantityData() => new TheoryData<int>()
    {
       PartConstant.MinQuantity - 1,
       PartConstant.MaxQuantity + 1,
    };
}
