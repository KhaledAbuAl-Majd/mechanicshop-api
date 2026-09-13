using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.AssignLabor;

[Collection(WebAppFactoryCollection.CollectionName)]
public class AssignLaborCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator = default!;
    private readonly IAppDbContext _context = default!;
    private readonly IServiceScope _scope = default!;

    private readonly WebAppFactory _factory;

    public AssignLaborCommandHandlerTests(WebAppFactory factory)
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
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var cancellationToken = CancellationToken.None;
        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, cancellationToken: cancellationToken, hoursOffset: 3);

        var labor = EmployeeFactory.CreateLabor().Value;
        _context.Employees.Add(labor);
        await _context.SaveChangesAsync(cancellationToken);

        var command = new AssignLaborCommand(workOrderDto.WorkOrderId, labor.Id);

        var result = await _mediator.Send(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
    }


    [Fact]
    public async Task Handle_ShouldFail_WhenWorkOrderNotFound()
    {
        var cancellationToken = CancellationToken.None;

        var labor = EmployeeFactory.CreateLabor().Value;
        _context.Employees.Add(labor);
        await _context.SaveChangesAsync(cancellationToken);

        var command = new AssignLaborCommand(Guid.NewGuid(), labor.Id);

        var result = await _mediator.Send(command, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLaborNotFound()
    {
        var cancellationToken = CancellationToken.None;
        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, cancellationToken: cancellationToken,spot:Spot.C);

        var command = new AssignLaborCommand(workOrderDto.WorkOrderId, Guid.NewGuid());

        var result = await _mediator.Send(command, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.LaborNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLaborConflict()
    {
        var cancellationToken = CancellationToken.None;

        //same time - different spots and labors
        var workOrderDto1 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, cancellationToken: cancellationToken, spot: Spot.A);
        var workOrderDto2 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, cancellationToken: cancellationToken, spot: Spot.B);

        var command = new AssignLaborCommand(workOrderDto2.WorkOrderId, workOrderDto1.Labor!.LaborId);

        var result = await _mediator.Send(command, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("Labor.Occupied", result.TopError.Code);
    }
}
