using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask
{
    public sealed class CreateRepairTaskCommandHandler(ILogger<CreateRepairTaskCommandHandler> logger, IAppDbContext context) : IRequestHandler<CreateRepairTaskCommand, Result<RepairTaskDto>>
    {
        private readonly ILogger<CreateRepairTaskCommandHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        public async Task<Result<RepairTaskDto>> Handle(CreateRepairTaskCommand command, CancellationToken ct)
        {
            var nameExists = await _context.RepairTasks.AnyAsync(p => EF.Functions.Like(p.Name, command.Name), ct);

            if (nameExists)
            {
                _logger.LogWarning("Duplicate repair task name '{Name}'.", command.Name);

                return RepairTaskErrors.DuplicateName;
            }

            List<Part> parts = [];

            foreach (var part in command.Parts)
            {
                var partResult = Part.Create(Guid.NewGuid(), part.Name.Trim(), part.Cost, part.Quantity);

                if (partResult.IsError)
                    return partResult.Errors;

                parts.Add(partResult.Value);
            }

            var createRepairTaskResult = RepairTask.Create(
                Guid.NewGuid(),
                command.Name!,
                command.LaborCost,
                command.EstimatedDurationInMins!.Value,
                parts);

            if (createRepairTaskResult.IsError)
                return createRepairTaskResult.Errors;

            var repairTask = createRepairTaskResult.Value;

            _context.RepairTasks.Add(repairTask);

            await _context.SaveChangesAsync(ct);


            _logger.LogInformation("RepairTask created successfully. Id: {RepairTaskId}", repairTask.Id);


            return repairTask.ToDto();

        }
    }
}
