using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.Customers.Common;
using MechanicShop.Domain.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.CreateCustomer;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateCustomerCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public CreateCustomerCommandHandlerTests(WebAppFactory factory)
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
    public async Task Handle_ShouldFail_WhenEmailAlreadyExists()
    {
        var ct = CancellationToken.None;

        var customer = await CustomerTestHelper.CreateValidCustomer(_context, ct);

        var command = new CreateCustomerCommand("new customer", "+23943843", customer.Email!, [new CreateVehicleCommand("dfdk", "test", 2025, "test|23535")]);
        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.CustomerEmailExists.Code, result.TopError.Code);
    }


    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;
        string email = "admin@gmail.com";
        var command = new CreateCustomerCommand("new customer", "+23943843", email, [new CreateVehicleCommand("dfdk", "test", 2025, "test|23535")]);
        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var customer = await _context.Customers.Include(c => c.Vehicles).FirstOrDefaultAsync(c => c.Email!.ToLower() == email.ToLower(), ct);
        Assert.NotNull(customer);
        Assert.Equal(customer.Id, result.Value.CustomerId);
        Assert.Single(customer.Vehicles);
        Assert.Equal(command.PhoneNumber, customer.PhoneNumber);
        Assert.Equal(command.Vehicles[0].Model, customer.Vehicles.First().Model);
        Assert.Equal(command.Vehicles[0].Make, customer.Vehicles.First().Make);
        Assert.Equal(command.Vehicles[0].Year, customer.Vehicles.First().Year);
        Assert.Equal(command.Vehicles[0].LicensePlate, customer.Vehicles.First().LicensePlate);
    }


}
