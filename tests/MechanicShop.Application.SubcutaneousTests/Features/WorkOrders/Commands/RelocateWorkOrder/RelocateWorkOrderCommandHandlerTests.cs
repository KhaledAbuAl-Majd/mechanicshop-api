using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.RelocateWorkOrder;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RelocateWorkOrderCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator = default!;
    private readonly IAppDbContext _context = default!;
    private readonly IServiceScope _scope = default!;

    private readonly WebAppFactory _factory;

    public RelocateWorkOrderCommandHandlerTests(WebAppFactory factory)
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
        var command = new RelocateWorkOrderCommand(Guid.NewGuid(), WorkOrderTestHelper.GetTomorrowOpening().AddHours(3), Spot.D);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
    }

    [Theory]
    [MemberData(nameof(GetInvalidStartAt))]
    public async Task Handle_ShouldFail_WhenOutsideOperatingHours((DateTimeOffset scheduledAt, int hoursOffset) data)
    {
        var ct = CancellationToken.None;

        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, hoursOffset: data.hoursOffset, spot: Spot.C, cancellationToken: ct);

        var command = new RelocateWorkOrderCommand(workOrderDto.WorkOrderId, data.scheduledAt, Spot.D);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderOutsideOperatingHour(command.NewStartAt, default).Code, result.TopError.Code);
    }


    [Fact]
    public async Task Handle_ShouldFail_WhenUnavailableSpot()
    {
        var ct = CancellationToken.None;

        var workOrderDto1 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 4);

        var workOrderDto2 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 5);

        var command = new RelocateWorkOrderCommand(workOrderDto2.WorkOrderId, workOrderDto1.StartAtUtc, workOrderDto1.Spot);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal("MechanicShop.Spot.Full", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenVehicleConflict()
    {
        var ct = CancellationToken.None;

        var customer = CustomerFactory.CreateCustomer().Value;

        var workOrderDto1 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 6, spot: Spot.C, customer: customer);
        var workOrderDto2 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 7, spot: Spot.B, customer: customer);

        var command = new RelocateWorkOrderCommand(workOrderDto2.WorkOrderId, workOrderDto1.StartAtUtc, workOrderDto2.Spot);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal("Vehicle.Overlapping.WorkOrders", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLaborConflict()
    {
        var ct = CancellationToken.None;

        var labor = EmployeeFactory.CreateLabor().Value;

        var workOrderDto1 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 4, spot: Spot.C, labor: labor);
        var workOrderDto2 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 6, Spot.B, labor: labor);

        var command = new RelocateWorkOrderCommand(workOrderDto2.WorkOrderId, workOrderDto1.StartAtUtc, workOrderDto2.Spot);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal("Labor.Occupied", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct);

        var command = new RelocateWorkOrderCommand(workOrderDto.WorkOrderId, WorkOrderTestHelper.GetTomorrowOpening().AddHours(5), Spot.B);

        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);

        var workOrder = await _context.WorkOrders.FirstOrDefaultAsync(wo => wo.Id == workOrderDto.WorkOrderId, ct);
        Assert.NotNull(workOrder);
        Assert.Equal(command.NewSpot, workOrder.Spot);
        Assert.Equal(command.NewStartAt, workOrder.StartAtUtc);
    }

    public static TheoryData<(DateTimeOffset, int)> GetInvalidStartAt => new TheoryData<(DateTimeOffset, int)>()
    {
        (new DateTimeOffset(WorkOrderTestHelper.GetTomorrow().ToDateTime(AppSettingsTestData.DefaultOpeningTime.AddHours(-1)),TimeSpan.Zero),1),
        (new DateTimeOffset(WorkOrderTestHelper.GetTomorrow().ToDateTime(AppSettingsTestData.DefaultClosingTime.AddHours(1)),TimeSpan.Zero),2)
    };
}
