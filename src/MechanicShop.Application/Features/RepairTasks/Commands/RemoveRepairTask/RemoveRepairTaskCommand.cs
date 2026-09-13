using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Constants;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask
{
    public sealed record RemoveRepairTaskCommand(Guid RepairTaskId) : IInvalidateCacheCommand<Result<Deleted>>
    {
        public string[] Tags => [RepairTaskCache.Tag];
    }
}
