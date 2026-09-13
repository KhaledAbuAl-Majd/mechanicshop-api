using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.Billing.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.SettleInvoice;

[Collection(WebAppFactoryCollection.CollectionName)]
public class SettleInvoiceCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public SettleInvoiceCommandHandlerTests(WebAppFactory factory)
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
    public async Task Handle_ShouldFail_WhenInvoiceNotFound()
    {
        var ct = CancellationToken.None;

        var command = new SettleInvoiceCommand(Guid.NewGuid());

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.InvoiceNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var expectedInvoice = await BillingTestHelper.CreateValidInvoice(_mediator, _context, ct);

        var command = new SettleInvoiceCommand(expectedInvoice.Id);

        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Success, result.Value);

        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == expectedInvoice.Id, ct);
        Assert.NotNull(invoice);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }
}
