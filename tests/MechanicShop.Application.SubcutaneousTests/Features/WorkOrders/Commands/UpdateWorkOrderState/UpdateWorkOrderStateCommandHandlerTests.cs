using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderState;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateWorkOrderStateCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;
    private readonly IServiceScope _scope;

    private readonly WebAppFactory _factory;

    public UpdateWorkOrderStateCommandHandlerTests(WebAppFactory factory)
    {
        _factory = factory;
        (_mediator, _context, _scope) = _factory.CreateMediatorAndAppDbContext();
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
        var command = new UpdateWorkOrderStateCommand(Guid.NewGuid(), WorkOrderState.InProgress);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
    }


    [Fact]
    public async Task Handle_ShouldFail_WhenWorkOrderNotStartedYet()
    {
        var ct = CancellationToken.None;

        DateOnly date = WorkOrderTestHelper.GetTomorrow().AddDays(3);

        _factory.FakeTimeProvider.SetUtcNow(new DateTimeOffset(date.ToDateTime(AppSettingsTestData.DefaultOpeningTime), TimeSpan.Zero));

        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(
         _mediator,
         _context,
         ct,
         hoursOffset: 0,
         startAt: _factory.FakeTimeProvider.GetUtcNow().AddMinutes(1));

        var command = new UpdateWorkOrderStateCommand(workOrderDto.WorkOrderId, WorkOrderState.InProgress);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.StateTransitionNotAllowed(workOrderDto.StartAtUtc).Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        DateOnly date = WorkOrderTestHelper.GetTomorrow().AddDays(3);

        _factory.FakeTimeProvider.SetUtcNow(new DateTimeOffset(date.ToDateTime(AppSettingsTestData.DefaultOpeningTime).AddHours(1), TimeSpan.Zero));

        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(
            _mediator,
            _context,
            ct,
            hoursOffset: 0,
            startAt: _factory.FakeTimeProvider.GetUtcNow().AddMinutes(-1));

        var command = new UpdateWorkOrderStateCommand(workOrderDto.WorkOrderId, WorkOrderState.InProgress);

        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);

        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(wo => wo.Id == workOrderDto.WorkOrderId, ct);

        Assert.NotNull(workOrder);
        Assert.Equal(command.State, workOrder.State);
    }
}
