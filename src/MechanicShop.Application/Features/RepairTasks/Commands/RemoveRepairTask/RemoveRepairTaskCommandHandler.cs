using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask
{
    public sealed class RemoveRepairTaskCommandHandler(ILogger<RemoveRepairTaskCommandHandler> logger,
        IAppDbContext context) : IRequestHandler<RemoveRepairTaskCommand, Result<Deleted>>
    {
        private readonly ILogger<RemoveRepairTaskCommandHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        public async Task<Result<Deleted>> Handle(RemoveRepairTaskCommand command, CancellationToken ct)
        {
            var repairTask = await _context.RepairTasks.FindAsync([command.RepairTaskId], ct);

            if (repairTask is null)
            {
                _logger.LogWarning("RepairTask {RepairTaskId} not found for deletion.", command.RepairTaskId);
                return ApplicationErrors.RepairTaskNotFound;
            }

            var inUse = await _context.WorkOrders.AnyAsync(wo => wo.RepairTasks.Any(rt => rt.Id == command.RepairTaskId),ct);

            //var inUse = await _context.WorkOrders.SelectMany(wo => wo.RepairTasks).AnyAsync(rt => rt.Id == command.RepairTaskId, ct);

            if (inUse)
            {
                _logger.LogWarning("RepairTask {RepairTaskId} cannot be deleted — in use by work orders.", command.RepairTaskId);
                return RepairTaskErrors.InUse;
            }

            _context.RepairTasks.Remove(repairTask);

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("RepairTask deleted successfully. Id: {RepairTaskId}", repairTask.Id);

            return Result.Deleted;
        }
    }
}
