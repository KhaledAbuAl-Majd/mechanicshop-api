using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.RepairTasks;

namespace MechanicShop.Domain.UnitTests.RepairTasks;

public class RepairTaskTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenIdEmpty()
    {
        var result = RepairTaskFactory.CreateRepairTask(id: Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.IdRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnError_WhenNameInvalid(string? name)
    {
        var result = RepairTaskFactory.CreateRepairTask(name: name);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [MemberData(nameof(InvalidLaborData))]
    public void Create_ShouldReturnError_WhenLaborCostInvalid(decimal laborCost)
    {
        var result = RepairTaskFactory.CreateRepairTask(laborCost: laborCost);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.LaborCostInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenDurationInvalid()
    {
        var duration = (RepairDurationInMinutes)999;

        var result = RepairTaskFactory.CreateRepairTask(estimatedDurationInMins: duration);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.DurationInvalid.Code, result.TopError.Code);
    }

    [Theory]
    [MemberData(nameof(InvalidPartsData))]
    public void Create_ShouldReturnError_WhenPartsInvalid(List<Part> parts)
    {
        var result = RepairTask.Create(
            id: Guid.NewGuid(),
            name: "oil change",
            laborCost: 75m,
            estimatedDurationInMins: RepairDurationInMinutes.Min30,
            parts: parts);


        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.PartsRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenPartsHaveDuplicateName()
    {
        var partName = "filter";

        List<Part> parts = [PartFactory.CreatePart(name: partName).Value, PartFactory.CreatePart(name: partName).Value];

        var result = RepairTaskFactory.CreateRepairTask(parts: parts);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.PartsDuplicateName.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidData()
    {
        var id = Guid.NewGuid();
        string name = "filter";
        decimal laborCost = 80m;
        RepairDurationInMinutes duration = RepairDurationInMinutes.Min45;
        List<Part> parts = [PartFactory.CreatePart(name: "part 1").Value, PartFactory.CreatePart(name: "part 2").Value];

        var totalCost = laborCost + parts.Sum(x => x.Quantity * x.Cost);

        var result = RepairTaskFactory.CreateRepairTask(
            id: id,
            name: name,
            laborCost: laborCost,
            estimatedDurationInMins: duration,
            parts: parts);


        Assert.True(result.IsSuccess);
        var repairTask = result.Value;
        Assert.NotNull(repairTask);
        Assert.Equal(id, repairTask.Id);
        Assert.Equal(name.Trim(), repairTask.Name.Trim());
        Assert.Equal(laborCost, repairTask.LaborCost);
        Assert.Equal(duration, repairTask.EstimatedDurationInMins);
        Assert.Equal(2, repairTask.Parts.Count());
        Assert.Equal(totalCost, repairTask.TotalCost);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ShouldReturnError_WhenNameInvalid(string? name)
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        decimal laborCost = 90m;
        RepairDurationInMinutes duration = RepairDurationInMinutes.Min60;

        var result = repairTask.Update(name!, laborCost, duration);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [MemberData(nameof(InvalidLaborData))]
    public void Update_ShouldReturnError_WhenLaborCostInvalid(decimal laborCost)
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        string name = "change oil";
        RepairDurationInMinutes duration = RepairDurationInMinutes.Min60;

        var result = repairTask.Update(name, laborCost, duration);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.LaborCostInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Update_ShouldReturnError_WhenDurationInvalid()
    {
        var duration = (RepairDurationInMinutes)999;

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        string name = "change oil";
        decimal laborCost = 90m;

        var result = repairTask.Update(name, laborCost, duration);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.DurationInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Update_ShouldReturnSuccess_WhenValidData()
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        string name = "change oil";
        decimal laborCost = 90m;
        RepairDurationInMinutes duration = RepairDurationInMinutes.Min60;

        var result = repairTask.Update(name, laborCost, duration);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(name.Trim(), repairTask.Name.Trim());
        Assert.Equal(laborCost, repairTask.LaborCost);
        Assert.Equal(duration, repairTask.EstimatedDurationInMins);
    }

    [Theory]
    [MemberData(nameof(InvalidPartsData))]
    public void UpsertParts_ShouldReturnError_WhenPartsInvalid(List<Part> parts)
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        var result = repairTask.UpsertParts(parts);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.PartsRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void UpsertParts_ShouldReturnError_WhenPartsHaveDuplicateName()
    {
        var partName = "filter";

        List<Part> parts = [PartFactory.CreatePart(name: partName).Value, PartFactory.CreatePart(name: partName).Value];

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        var result = repairTask.UpsertParts(parts);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.PartsDuplicateName.Code, result.TopError.Code);
    }

    [Fact]
    public void UpsertParts_ShouldReturnSuccess_WhenRemovingPartNotInIncomingList()
    {

        var part1 = PartFactory.CreatePart(name: "oil filter").Value;
        var part2 = PartFactory.CreatePart(name:"oil").Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(parts: [part1, part2]).Value;

        var newName = "filter";

        List<Part> IncomingParts = [PartFactory.CreatePart(id: part1.Id, name: newName).Value, PartFactory.CreatePart().Value];

        var newTotal = repairTask.LaborCost + IncomingParts.Sum(x => x.Cost * x.Quantity);

        var result = repairTask.UpsertParts(IncomingParts);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(2, repairTask.Parts.Count);
        Assert.Equal(newTotal, repairTask.TotalCost);
        Assert.Contains(repairTask.Parts, p => p.Id == part1.Id && p.Name!.Trim() == newName.Trim());
    }


    public static TheoryData<decimal> InvalidLaborData => new TheoryData<decimal>()
    {
        RepairTaskConstant.MinLaborCost - 1,
        RepairTaskConstant.MaxLaborCost + 1,
    };
    public static TheoryData<List<Part>> InvalidPartsData => new TheoryData<List<Part>>()
    {
        null!,
        new List<Part>()
    };
}
