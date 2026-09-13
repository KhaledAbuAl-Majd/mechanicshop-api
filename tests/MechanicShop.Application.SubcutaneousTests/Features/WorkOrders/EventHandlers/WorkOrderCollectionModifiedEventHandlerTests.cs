using MechanicShop.Application.Common;
using MechanicShop.Application.Features.WorkOrders.EventHandlers;
using MechanicShop.Domain.WorkOrders.Events;
using NSubstitute;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.EventHandlers;

public class WorkOrderCollectionModifiedEventHandlerTests
{
    private readonly IWorkOrderNotifier _notifier = Substitute.For<IWorkOrderNotifier>();
    private readonly WorkOrderCollectionModifiedEventHandler _sut;

    public WorkOrderCollectionModifiedEventHandlerTests()
    {
        _sut = new WorkOrderCollectionModifiedEventHandler(_notifier);
    }

    [Fact]
    public async Task ShouldNotifyWorkOrderChanged()
    {
        var ct = CancellationToken.None;

        var domainEvent = new WorkOrderCollectionModified();

        await _sut.Handle(domainEvent, ct);

        await _notifier.Received(1).NotifyWorkOrdersChangedAsync(Arg.Any<CancellationToken>());
    }
}
