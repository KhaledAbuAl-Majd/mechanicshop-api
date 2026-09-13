using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.UpdateRepairTask;

public class UpdateRepairTaskCommandValidatorTests
{
    private readonly UpdateRepairTaskCommandValidator _validator = new();


    [Fact]
    public async Task Validate_ShouldFail_WhenIdInvalid()
    {
        var ct = CancellationToken.None;

        var partCommand = new UpdateRepairTaskPartCommand(Guid.NewGuid(), Name: "oilFilter-1234", Cost: 40, Quantity: 3);

        var command = new UpdateRepairTaskCommand(
            Guid.Empty,
            Name: "change oil"!,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            LaborCost: 30,
            Parts: [partCommand]);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(RepairTaskErrors.IdRequired.Code, result.Errors[0].ErrorCode);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_ShouldFail_WhenNameInvalid(string? name)
    {
        var ct = CancellationToken.None;

        var partCommand = new UpdateRepairTaskPartCommand(Guid.NewGuid(), Name: "oilFilter-1234", Cost: 40, Quantity: 3);

        var command = new UpdateRepairTaskCommand(
            Guid.NewGuid(),
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

        var partCommand = new UpdateRepairTaskPartCommand(Guid.NewGuid(), Name: "oilFilter-1234", Cost: 40, Quantity: 3);

        var command = new UpdateRepairTaskCommand(Guid.NewGuid(),
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

        var partCommand = new UpdateRepairTaskPartCommand(Guid.NewGuid(), Name: "oilFilter-1234", Cost: 40, Quantity: 3);

        var command = new UpdateRepairTaskCommand(Guid.NewGuid(),
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
    public async Task Validate_ShouldFail_WhenPartsInvalid(List<UpdateRepairTaskPartCommand>? parts)
    {
        var ct = CancellationToken.None;

        var command = new UpdateRepairTaskCommand(Guid.NewGuid(),
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

        var partCommand = new UpdateRepairTaskPartCommand(Guid.NewGuid(), Name: "oilFilter-1234", Cost: 40, Quantity: 3);

        var command = new UpdateRepairTaskCommand(Guid.NewGuid(),
            Name: "change oil"!,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            LaborCost: 30,
            Parts: [partCommand]);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }


    public static TheoryData<List<UpdateRepairTaskPartCommand>?> GetInvalidParts => new()
    {
        null,
        new List<UpdateRepairTaskPartCommand>()
    };
}
