using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.WorkOrders.Billing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Common;

public static class BillingTestHelper
{

    public static async Task<Invoice> CreateValidInvoice(
        IMediator mediator,
        IAppDbContext context,
        CancellationToken ct = default,
        WorkOrder? workOrder = null,
        int hoursOffset = 0,
        Spot spot = Spot.D,
        TimeProvider? provider = null)
    {
        provider ??= TimeProvider.System;

        if (workOrder is null)
        {
            var startAt = WorkOrderTestHelper.GetTomorrowOpening(provider.GetUtcNow().UtcDateTime);

            var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(
                 mediator,
                 context,
                 ct,
                 hoursOffset: hoursOffset,
                 spot: spot,
                 startAt: startAt);

            workOrder = await context.WorkOrders.SingleAsync(wo => wo.Id == workOrderDto.WorkOrderId, ct);
        }

        if (workOrder.State == WorkOrderState.Scheduled)
        {
            workOrder.UpdateState(WorkOrderState.InProgress);
        }

        if (workOrder.State == WorkOrderState.InProgress)
        {
            workOrder.UpdateState(WorkOrderState.Completed);
        }


        await context.SaveChangesAsync(ct);

        var invoice = InvoiceFactory.CreateInvoice(
            workOrderId: workOrder.Id,
            datetime: provider).Value;


        context.Invoices.Add(invoice);

        await context.SaveChangesAsync(ct);

        return invoice;
    }
}
