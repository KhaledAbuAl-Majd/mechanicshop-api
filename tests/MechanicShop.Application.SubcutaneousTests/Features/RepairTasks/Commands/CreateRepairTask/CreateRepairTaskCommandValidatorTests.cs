using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.CreateRepairTask;

public class CreateRepairTaskCommandValidatorTests
{
    private readonly CreateRepairTaskCommandValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_ShouldFail_WhenNameInvalid(string? name)
    {
        var ct = CancellationToken.None;

        var partCommand = new CreateRepairTaskPartCommand(Name: "oilFilter-1234", Cost: 40, Quantity: 3);

        var command = new CreateRepairTaskCommand(
            Name: name!,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            LaborCost: 30,
            Parts: [partCommand]);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(RepairTaskErrors.NameRequired.Code, result.Errors[0].ErrorCode);
    }


    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task Validate_ShouldFail_WhenLaborCostInvalid(decimal laborCost)
    {
        var ct = CancellationToken.None;

        var partCommand = new CreateRepairTaskPartCommand(Name: "oilFilter-1234", Cost: 40, Quantity: 3);

        var command = new CreateRepairTaskCommand(
            Name: "change oil"!,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            LaborCost: laborCost,
            Parts: [partCommand]);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(nameof(command.LaborCost), result.Errors[0].PropertyName);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenEstimatedDurationInvalid()
    {
        var ct = CancellationToken.None;

        var partCommand = new CreateRepairTaskPartCommand(Name: "oilFilter-1234", Cost: 40, Quantity: 3);

        var command = new CreateRepairTaskCommand(
            Name: "change oil"!,
            EstimatedDurationInMins: (RepairDurationInMinutes)9999,
            LaborCost: 30,
            Parts: [partCommand]);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(nameof(command.EstimatedDurationInMins), result.Errors[0].PropertyName);
    }

    [Theory]
    [MemberData(nameof(GetInvalidParts))]
    public async Task Validate_ShouldFail_WhenPartsInvalid(List<CreateRepairTaskPartCommand>? parts)
    {
        var ct = CancellationToken.None;

        var command = new CreateRepairTaskCommand(
            Name: "change oil"!,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            LaborCost: 30,
            Parts: parts!);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(nameof(command.Parts), result.Errors[0].PropertyName);
    }

    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var partCommand = new CreateRepairTaskPartCommand(Name: "oilFilter-1234", Cost: 40, Quantity: 3);

        var command = new CreateRepairTaskCommand(
            Name: "change oil"!,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            LaborCost: 30,
            Parts: [partCommand]);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }


    public static TheoryData<List<CreateRepairTaskPartCommand>?> GetInvalidParts => new()
    {
        null,
        new List<CreateRepairTaskPartCommand>()
    };
}
