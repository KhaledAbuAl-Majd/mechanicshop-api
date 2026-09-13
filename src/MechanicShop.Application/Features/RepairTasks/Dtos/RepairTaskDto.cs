using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.Features.RepairTasks.Dtos
{
    public record RepairTaskDto(
        Guid RepairTaskId,
        string Name,
        RepairDurationInMinutes EstimatedDurationInMins,
        decimal LaborCost,
        decimal TotalCost,
        IReadOnlyCollection<PartDto> Parts);
}
