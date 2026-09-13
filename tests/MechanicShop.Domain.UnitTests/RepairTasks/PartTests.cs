using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.RepairTasks;

namespace MechanicShop.Domain.UnitTests.RepairTasks;

public class PartTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenIdEmpty()
    {
        var result = PartFactory.CreatePart(id: Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(PartErrors.IdRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void Create_ShouldReturnError_WhenNameInvalid(string? name)
    {
        var result = PartFactory.CreatePart(name: name);

        Assert.False(result.IsSuccess);
        Assert.Equal(PartErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [MemberData(nameof(InvalidCostData))]
    public void Create_ShouldReturnError_WhenCostInvalid(decimal cost)
    {
        var result = PartFactory.CreatePart(cost: cost);

        Assert.False(result.IsSuccess);
        Assert.Equal(PartErrors.CostInvalid.Code, result.TopError.Code);
    }

    [Theory]
    [MemberData(nameof(InvalidQuantityData))]
    public void Create_ShouldReturnError_WhenQuantityInvalid(int quantity)
    {
        var result = PartFactory.CreatePart(quantity: quantity);

        Assert.False(result.IsSuccess);
        Assert.Equal(PartErrors.QuantityInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WithValidData()
    {
        Guid id = Guid.NewGuid();
        string name = "oil filter".Trim();
        decimal cost = 10m;
        int quantity = 2;

        var result = PartFactory.CreatePart(id: id, name: name, cost: cost, quantity: quantity);

        Assert.True(result.IsSuccess);
        var part = result.Value;
        Assert.NotNull(part);
        Assert.Equal(id, part.Id);
        Assert.Equal(name, part.Name);
        Assert.Equal(cost, part.Cost);
        Assert.Equal(quantity, part.Quantity);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void Update_ShouldReturnError_WhenNameInvalid(string? name)
    {
        var part = PartFactory.CreatePart().Value;

        decimal cost = 70;
        int quantity = 4;

        var result = part.Update(name!, cost, quantity);

        Assert.False(result.IsSuccess);
        Assert.Equal(PartErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [MemberData(nameof(InvalidCostData))]
    public void Update_ShouldReturnError_WhenCostInvalid(decimal cost)
    {
        var part = PartFactory.CreatePart().Value;

        string name = "oil filter".Trim();
        int quantity = 4;

        var result = part.Update(name, cost, quantity);

        Assert.False(result.IsSuccess);
        Assert.Equal(PartErrors.CostInvalid.Code, result.TopError.Code);
    }

    [Theory]
    [MemberData(nameof(InvalidQuantityData))]
    public void Update_ShouldReturnError_WhenQuantityInvalid(int quantity)
    {
        var part = PartFactory.CreatePart().Value;

        string name = "oil filter".Trim();
        decimal cost = 70;

        var result = part.Update(name, cost, quantity);

        Assert.False(result.IsSuccess);
        Assert.Equal(PartErrors.QuantityInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Update_ShouldReturnSuccess_WithValidData()
    {

        var part = PartFactory.CreatePart().Value;

        string name = "oil filter".Trim();
        decimal cost = 10m;
        int quantity = 2;

        var result = part.Update(name, cost, quantity);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(name, part.Name);
        Assert.Equal(cost, part.Cost);
        Assert.Equal(quantity, part.Quantity);
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
