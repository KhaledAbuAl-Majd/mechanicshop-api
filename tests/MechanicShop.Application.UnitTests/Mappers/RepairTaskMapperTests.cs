using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Tests.Common.RepairTasks;

namespace MechanicShop.Application.UnitTests.Mappers;

public class RepairTaskMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var RepairTask = RepairTaskFactory.CreateRepairTask().Value;

        var dto = RepairTask.ToDto();

        Assert.NotNull(dto);
        Assert.Equal(RepairTask.Id, dto.RepairTaskId);
        Assert.Equal(RepairTask.Name, dto.Name);
        Assert.Equal(RepairTask.EstimatedDurationInMins, dto.EstimatedDurationInMins);
        Assert.Equal(RepairTask.LaborCost, dto.LaborCost);
        Assert.Equal(RepairTask.TotalCost, dto.TotalCost);
        Assert.Equal(RepairTask.Parts.Count(), dto.Parts.Count);
    }

    [Fact]
    public void ToDto_ShouldThrowException_WhenRepairTaskIsNull()
    {
        var RepairTask = (RepairTask)null!;

        Assert.Throws<ArgumentNullException>(RepairTask.ToDto);
    }

    [Fact]
    public void ToDto_ShouldThrowException_WhenPartIsNull()
    {
        var RepairTask = RepairTaskFactory.CreateRepairTask(parts: null, setListIfNull: false).Value;

        Assert.Throws<ArgumentNullException>(RepairTask.ToDto);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        var RepairTask = RepairTaskFactory.CreateRepairTask().Value;
        List<RepairTask> RepairTasks = [RepairTask];

        var dtos = RepairTasks.ToDtos();
        Assert.NotNull(dtos);
        Assert.Single(dtos);

        var dto = dtos[0];
        Assert.Equal(RepairTask.Id, dto.RepairTaskId);
        Assert.Equal(RepairTask.Name, dto.Name);
        Assert.Equal(RepairTask.EstimatedDurationInMins, dto.EstimatedDurationInMins);
        Assert.Equal(RepairTask.LaborCost, dto.LaborCost);
        Assert.Equal(RepairTask.TotalCost, dto.TotalCost);
        Assert.Equal(RepairTask.Parts.Count(), dto.Parts.Count);
    }
}
