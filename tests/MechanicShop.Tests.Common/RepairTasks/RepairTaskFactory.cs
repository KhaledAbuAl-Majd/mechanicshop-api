using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Tests.Common.RepairTasks;

public static class RepairTaskFactory
{
    public static Result<RepairTask> CreateRepairTask(
        Guid? id = null,
        string? name = "Oil Change",
        decimal? laborCost = null,
        RepairDurationInMinutes? estimatedDurationInMins = null,
        List<Part>? parts = null, bool setListIfNull = true)
    {
        return RepairTask.Create(
            id ?? Guid.NewGuid(),
            name!,
            laborCost ?? 75m,
            estimatedDurationInMins ?? RepairDurationInMinutes.Min30,
            setListIfNull? parts ?? [PartFactory.CreatePart().Value]: parts!);
    }
}
