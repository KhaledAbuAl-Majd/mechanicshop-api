using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Constants;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask
{
    public sealed record UpdateRepairTaskCommand(
        Guid RepairTaskId,
        string Name,
        decimal LaborCost,
        RepairDurationInMinutes EstimatedDurationInMins,
        List<UpdateRepairTaskPartCommand> Parts) : IInvalidateCacheCommand<Result<Updated>>
    {
        public string[] Tags => [RepairTaskCache.Tag];
    }


}
