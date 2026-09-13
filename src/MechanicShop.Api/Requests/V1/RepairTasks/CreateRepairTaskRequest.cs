using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Api.Requests.V1.RepairTasks
{
    public sealed record CreateRepairTaskRequest(
        string Name,
        RepairDurationInMinutes? EstimatedDurationInMins,
        decimal LaborCost,
        List<CreateRepairTaskPartRequest> Parts);
}
