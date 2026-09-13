using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrderById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrderByIdQueryHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public GetWorkOrderByIdQueryHandlerTests(WebAppFactory factory)
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
        var query = new GetWorkOrderByIdQuery(Guid.NewGuid());

        var result = await _mediator.Send(query, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var createdWorkOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct);

        var query = new GetWorkOrderByIdQuery(createdWorkOrderDto.WorkOrderId);

        var result = await _mediator.Send(query, ct);

        Assert.True(result.IsSuccess);
        var dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(createdWorkOrderDto.WorkOrderId, dto.WorkOrderId);
        Assert.Equal(createdWorkOrderDto.State, dto.State);
        Assert.Equal(createdWorkOrderDto.Spot, dto.Spot);
        Assert.Equal(createdWorkOrderDto.TotalCost, dto.TotalCost);
    }
}
