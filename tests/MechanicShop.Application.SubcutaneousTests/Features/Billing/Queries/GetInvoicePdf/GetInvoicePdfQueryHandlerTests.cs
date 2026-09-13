using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.Billing.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoicePdf;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetInvoicePdfQueryHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public GetInvoicePdfQueryHandlerTests(WebAppFactory factory)
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

        var query = new GetInvoicePdfQuery(Guid.NewGuid());

        var result = await _mediator.Send(query, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.InvoiceNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var expectedInvoice = await BillingTestHelper.CreateValidInvoice(_mediator, _context, ct);

        var command = new GetInvoicePdfQuery(expectedInvoice.Id);

        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        var pdfDto = result.Value;
        Assert.NotNull(pdfDto);
        Assert.NotNull(pdfDto.Content!);
        Assert.NotEmpty(pdfDto.Content!);
    }
}
