using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.Billing.Common;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.IssueInvoice;

[Collection(WebAppFactoryCollection.CollectionName)]
public class IssueInvoiceCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public IssueInvoiceCommandHandlerTests(WebAppFactory factory)
    {
        _factory = factory;

        (_mediator, _context, _scope) = factory.CreateMediatorAndAppDbContext();
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }


    [Fact]
    public async Task Handle_ShouldFail_WhenWorkOrderNotFound()
    {
        var ct = CancellationToken.None;

        var command = new IssueInvoiceCommand(Guid.NewGuid());

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
    }


    [Fact]
    public async Task Handle_ShouldFail_WhenInvoiceAlreadyExists()
    {
        var ct = CancellationToken.None;

        var invoice1 = await BillingTestHelper.CreateValidInvoice(_mediator, _context, ct, hoursOffset: 0);

        var command = new IssueInvoiceCommand(invoice1.WorkOrderId);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.InvoiceAlreadyIssued.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(WorkOrderState.Scheduled)]
    [InlineData(WorkOrderState.InProgress)]
    [InlineData(WorkOrderState.Cancelled)]
    public async Task Handle_ShouldFail_WhenWorkOrderNotCompleted(WorkOrderState state)
    {
        var ct = CancellationToken.None;

        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 1);
        var workOrder = await _context.WorkOrders.SingleAsync(wo => wo.Id == workOrderDto.WorkOrderId, ct);

        if (state == WorkOrderState.InProgress)
        {
            workOrder.UpdateState(WorkOrderState.InProgress);
        }

        if (state == WorkOrderState.Cancelled)
        {
            workOrder.UpdateState(WorkOrderState.Cancelled);
        }

        await _context.SaveChangesAsync(ct);

        var command = new IssueInvoiceCommand(workOrderDto.WorkOrderId);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderMustBeCompletedForInvoicing.Code, result.TopError.Code);
    }


    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 1);
        var workOrder = await _context.WorkOrders.SingleAsync(wo => wo.Id == workOrderDto.WorkOrderId, ct);

        workOrder.UpdateState(WorkOrderState.InProgress);
        workOrder.UpdateState(WorkOrderState.Completed);

        await _context.SaveChangesAsync(ct);

        var command = new IssueInvoiceCommand(workOrderDto.WorkOrderId);

        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        var invoiceDto = result.Value;
        Assert.NotNull(invoiceDto);

        var invoice = await _context.Invoices.Include(i => i.LineItems).FirstOrDefaultAsync(i => i.Id == invoiceDto.InvoiceId);
        Assert.NotNull(invoice);
        Assert.Equal(workOrderDto.WorkOrderId, invoice.WorkOrderId);
        Assert.Equal(invoiceDto.WorkOrderId, invoice.WorkOrderId);
        Assert.Equal(invoiceDto.Items.Count, invoice.LineItems.Count);
        Assert.Equal(invoiceDto.Total, invoice.Total);
        Assert.Equal(invoiceDto.DiscountAmount, invoice.DiscountAmount);
        Assert.Equal(invoiceDto.TaxAmount, invoice.TaxAmount);
    }
}
