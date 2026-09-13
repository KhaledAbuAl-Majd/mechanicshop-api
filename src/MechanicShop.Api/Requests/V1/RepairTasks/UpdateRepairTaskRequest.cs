using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Api.Requests.V1.RepairTasks
{
    public sealed record UpdateRepairTaskRequest(
        string Name,
        decimal LaborCost,
        RepairDurationInMinutes EstimatedDurationInMins,
        List<UpdateRepairTaskPartRequest> Parts);

}
