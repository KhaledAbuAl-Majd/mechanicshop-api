using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.EventHandlers;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.EventHandlers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class SendWorkOrderCompletedEmailHandlerTests:IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    private readonly ILogger<SendWorkOrderCompletedEmailHandler> _logger = Substitute.For<ILogger<SendWorkOrderCompletedEmailHandler>>();
    private readonly SendWorkOrderCompletedEmailHandler _sut;

    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();

    public SendWorkOrderCompletedEmailHandlerTests(WebAppFactory factory)
    {
        _factory = factory;

        (_mediator, _context, _scope) = factory.CreateMediatorAndAppDbContext();

        _sut = new SendWorkOrderCompletedEmailHandler(_notificationService, _context, _logger);
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

        var domainEvent = new WorkOrderCompleted
        {
            WorkOrderId = Guid.NewGuid()
        };

        await _sut.Handle(domainEvent, ct);

        await _notificationService.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _notificationService.DidNotReceive().SendSmsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSendEmailAndSms_WhenCustomerHasEmailAndPhone()
    {
        var ct = CancellationToken.None;
        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct);

        var domainEvent = new WorkOrderCompleted
        {
            WorkOrderId = workOrderDto.WorkOrderId
        };

        await _sut.Handle(domainEvent, ct);

        var customer = await _context.WorkOrders.Where(wo => wo.Id == workOrderDto.WorkOrderId).Select(wo => wo.Vehicle!.Customer).FirstAsync(ct);

        Assert.NotNull(customer);
        await _notificationService.Received(1).SendEmailAsync(customer.Email!, Arg.Any<CancellationToken>());
        await _notificationService.Received(1).SendSmsAsync(customer.PhoneNumber!, Arg.Any<CancellationToken>());
    }
}
