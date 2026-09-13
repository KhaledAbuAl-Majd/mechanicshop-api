using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor
{
    public sealed class AssignLaborCommandHandler(
        ILogger<AssignLaborCommandHandler> logger,
        IAppDbContext context,
        IWorkOrderPolicy WorkOrderRuleService) : IRequestHandler<AssignLaborCommand, Result<Updated>>
    {
        private readonly ILogger<AssignLaborCommandHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        private readonly IWorkOrderPolicy _workOrderValidator = WorkOrderRuleService;
        public async Task<Result<Updated>> Handle(AssignLaborCommand command, CancellationToken ct)
        {
            var workOrder = await _context.WorkOrders.FindAsync([command.WorkOrderId], ct);

            if (workOrder is null)
            {
                _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", command.WorkOrderId);
                return ApplicationErrors.WorkOrderNotFound;
            }

            var laborExists = await _context.Employees.AnyAsync(e => e.Id == command.LaborId, ct);

            if (!laborExists)
            {
                _logger.LogError("Invalid LaborId: {LaborId}", command.LaborId);
                return ApplicationErrors.LaborNotFound;
            }

            var checkLaborOccupied = await _workOrderValidator.IsLaborOccupied(command.LaborId, command.WorkOrderId, workOrder.StartAtUtc, workOrder.EndAtUtc, ct);

            if (checkLaborOccupied.IsError)
            {
                _logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", command.LaborId);
                return checkLaborOccupied.Errors;
            }

            var updateLaborResult = workOrder.UpdateLabor(command.LaborId);

            if (updateLaborResult.IsError)
            {
                foreach (var error in updateLaborResult.Errors)
                {
                    _logger.LogError("[LaborUpdate] {ErrorCode}: {ErrorDescription}", error.Code, error.Description);
                }

                return updateLaborResult.Errors;
            }

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("WorkOrder with Id '{WorkOrderId}' assigned labor '{LaborId}' successfully.", workOrder.Id, workOrder.LaborId);

            return Result.Updated;
        }
    }
}
