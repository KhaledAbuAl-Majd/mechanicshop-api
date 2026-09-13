using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Constants;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask
{
    public sealed record CreateRepairTaskCommand(
        string? Name,
        RepairDurationInMinutes? EstimatedDurationInMins,
        decimal LaborCost,
        List<CreateRepairTaskPartCommand> Parts) : IInvalidateCacheCommand<Result<RepairTaskDto>>
    {
        public string[] Tags => [RepairTaskCache.Tag];
    }
}
