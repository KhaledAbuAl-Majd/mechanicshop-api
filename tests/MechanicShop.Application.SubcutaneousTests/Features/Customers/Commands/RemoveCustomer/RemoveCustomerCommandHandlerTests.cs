using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.Customers.Common;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Tests.Common.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.RemoveCustomer;

[Collection(WebAppFactoryCollection.CollectionName)]

public class RemoveCustomerCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public RemoveCustomerCommandHandlerTests(WebAppFactory factory)
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
    public async Task Handle_ShouldFail_WhenCustomerNotFound()
    {
        var ct = CancellationToken.None;

        var command = new RemoveCustomerCommand(Guid.NewGuid());

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.CustomerNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCustomerAssociatedWorkOrders()
    {
        var ct = CancellationToken.None;

        var customer = CustomerFactory.CreateCustomer().Value;

        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, customer: customer);

        var command = new RemoveCustomerCommand(customer.Id);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.CannotDeleteCustomerWithWorkOrders.Code, result.TopError.Code);
    }


    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var customer = await CustomerTestHelper.CreateValidCustomer(_context, ct);

        var command = new RemoveCustomerCommand(customer.Id);

        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Deleted, result.Value);

        var exists = await _context.Customers.AnyAsync(c => c.Id == customer.Id, ct);
        Assert.False(exists);
    }

}
