using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.Customers.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.UpdateCustomer;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateCustomerCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public UpdateCustomerCommandHandlerTests(WebAppFactory factory)
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

        var command = new UpdateCustomerCommand(
            Guid.NewGuid(),
            "khaled",
            "+3843433",
            "khae@gmail.com",
            [new UpdateVehicleCommand(Guid.NewGuid(), "dfdk", "test", 2025, "test|23535")]);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.CustomerNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenEmailAlreadyExists()
    {
        var ct = CancellationToken.None;

        var customer1 = await CustomerTestHelper.CreateValidCustomer(_context, ct);

        var customer2 = await CustomerTestHelper.CreateValidCustomer(_context, ct);

        var command = new UpdateCustomerCommand(
           customer2.Id,
            customer2.Name!,
            customer2.PhoneNumber!,
            customer1.Email!,
            [new UpdateVehicleCommand(customer2.Vehicles.First().Id, "dfdk", "test", 2025, "test|23535")]);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.CustomerEmailExists.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var createdCustomer = await CustomerTestHelper.CreateValidCustomer(_context, ct);
        var vehicle = createdCustomer.Vehicles.First();

        var command = new UpdateCustomerCommand(
            createdCustomer.Id,
            "khaled",
            "+3843433",
            "khae@gmail.com",
            [new UpdateVehicleCommand(vehicle.Id, "mercedies", "test", 2010, vehicle.LicensePlate)]);

        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);

        var updatedCustomer = await _context.Customers.Include(c => c.Vehicles).FirstOrDefaultAsync(c => c.Id == command.CustomerId, ct);
        Assert.NotNull(updatedCustomer);
        Assert.Equal(command.Vehicles.Count, updatedCustomer.Vehicles.Count());
        Assert.Equal(command.PhoneNumber, updatedCustomer.PhoneNumber);
        Assert.Equal(command.Email, updatedCustomer.Email);
        Assert.Equal(command.Name, updatedCustomer.Name);
        Assert.Equal(command.Vehicles[0].Model, updatedCustomer.Vehicles.First().Model);
        Assert.Equal(command.Vehicles[0].Make, updatedCustomer.Vehicles.First().Make);
        Assert.Equal(command.Vehicles[0].Year, updatedCustomer.Vehicles.First().Year);
        Assert.Equal(command.Vehicles[0].LicensePlate, updatedCustomer.Vehicles.First().LicensePlate);
    }
}
