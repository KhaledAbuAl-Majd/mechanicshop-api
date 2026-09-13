using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.Common.Constants;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Billing.InvoiceLineItems;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice
{
    public sealed class IssueInvoiceCommandHandler(
        ILogger<IssueInvoiceCommandHandler> logger,
        IAppDbContext context,
        TimeProvider datetime) : IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
    {

        private readonly ILogger<IssueInvoiceCommandHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        private readonly TimeProvider _datetime = datetime;
        public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand command, CancellationToken ct)
        {
            var workOrder = await _context.WorkOrders.Include(wo => wo.Vehicle!)
                .ThenInclude(v => v.Customer)
                .Include(wo => wo.RepairTasks)
                .ThenInclude(rt => rt.Parts)
                .FirstOrDefaultAsync(wo => wo.Id == command.WorkOrderId, ct);


            if (workOrder is null)
            {
                _logger.LogWarning("Invoice issuance failed. WorkOrder {WorkOrderId} not found.", command.WorkOrderId);

                return ApplicationErrors.WorkOrderNotFound;
            }

            var invoiceExists = await _context.Invoices.AnyAsync(i => i.WorkOrderId == workOrder.Id, ct);

            if (invoiceExists)
            {
                _logger.LogWarning("Invoice issuance rejected. Invoice already exists for WorkOrder {WorkOrderId}.", command.WorkOrderId);
                return ApplicationErrors.InvoiceAlreadyIssued;
            }


            if (workOrder.State != WorkOrderState.Completed)
            {
                _logger.LogWarning("Invoice issuance rejected. WorkOrder {WorkOrderId} is not in completed.", command.WorkOrderId);

                return ApplicationErrors.WorkOrderMustBeCompletedForInvoicing;
            }

            Guid invoiceId = Guid.NewGuid();

            var lineItems = new List<InvoiceLineItem>();

            int lineNumber = 1;

            foreach (var (task, taskIndex) in workOrder.RepairTasks.Select((rt, i) => (rt, i + 1)))
            {
                var partsSummary = task.Parts.Any()
                    ? string.Join(Environment.NewLine, task.Parts.Select(p => $"    • {p.Name} x {p.Quantity} @ {p.Cost:C}"))
                    : "    • No parts";

                var lineDescription =
                    $"{taskIndex}: {task.Name} {Environment.NewLine}" +
                    $"  Labor = {task.LaborCost:C} {Environment.NewLine}" +
                    $"  Parts: {Environment.NewLine}{partsSummary}";

                var totalPartsCost = task.Parts.Sum(p => p.Cost * p.Quantity);
                var totalTaskCost = task.LaborCost + totalPartsCost;

                var lineItemReslut = InvoiceLineItem.Create(
                    invoiceId,
                    lineNumber++,
                    lineDescription,
                    1,
                    totalTaskCost);

                if (lineItemReslut.IsError)
                    return lineItemReslut.Errors;

                lineItems.Add(lineItemReslut.Value);
            }

            var subTotal = lineItems.Sum(x => x.LineTotal);

            var taxAmount = subTotal * MechanicShopConstants.TaxRate;

            var discountAmount = workOrder.Discount ?? 0m;

            var createInvoiceResult = Invoice.Create(invoiceId, workOrder.Id, lineItems, discountAmount, taxAmount, _datetime);

            if (createInvoiceResult.IsError)
            {
                _logger.LogWarning(
                     "Invoice creation failed for WorkOrderId: {WorkOrderId}. Errors: {@Errors}",
                     command.WorkOrderId,
                     createInvoiceResult.Errors);

                return createInvoiceResult.Errors;

            }

            var invoice = createInvoiceResult.Value;

            invoice.WorkOrder = workOrder;

            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Invoice {InvoiceId} issued for WorkOrder {WorkOrderId}.", invoice.Id, workOrder.Id);

            return invoice.ToDto();

        }
    }
}
