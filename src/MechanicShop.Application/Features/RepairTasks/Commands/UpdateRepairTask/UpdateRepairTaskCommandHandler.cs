using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask
{
    public sealed class UpdateRepairTaskCommandHandler(ILogger<UpdateRepairTaskCommandHandler> logger, IAppDbContext context) : IRequestHandler<UpdateRepairTaskCommand, Result<Updated>>
    {
        private readonly ILogger<UpdateRepairTaskCommandHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        public async Task<Result<Updated>> Handle(UpdateRepairTaskCommand command, CancellationToken ct)
        {
            var repairTask = await _context.RepairTasks.Include(rt => rt.Parts).FirstOrDefaultAsync(rt => rt.Id == command.RepairTaskId, ct);

            if (repairTask is null)
            {
                _logger.LogWarning("RepairTask {RepairTaskId} not found for update.", command.RepairTaskId);

                return ApplicationErrors.RepairTaskNotFound;
            }

            var nameExists = await _context.RepairTasks.AnyAsync(p => p.Id != command.RepairTaskId && EF.Functions.Like(p.Name, command.Name), ct);

            if (nameExists)
            {
                _logger.LogWarning("Duplicate repair task name '{Name}'.", command.Name);

                return RepairTaskErrors.DuplicateName;
            }

            List<Part> validatedParts = [];

            foreach (var p in command.Parts)
            {
                var partId = p.PartId ?? Guid.NewGuid();

                var createPartResult = Part.Create(partId, p.Name, p.Cost, p.Quantity);

                if (createPartResult.IsError)
                    return createPartResult.Errors;

                validatedParts.Add(createPartResult.Value);
            }

            var UpdateRepairtTaskResult = repairTask.Update(command.Name, command.LaborCost, command.EstimatedDurationInMins);

            if (UpdateRepairtTaskResult.IsError)
                return UpdateRepairtTaskResult.Errors;

            var upserPartsResult = repairTask.UpsertParts(validatedParts);

            if (upserPartsResult.IsError)
                return upserPartsResult.Errors;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("RepairTask updated successfully. Id: {RepairTaskId}", repairTask.Id);

            return Result.Updated;
        }
    }
}
