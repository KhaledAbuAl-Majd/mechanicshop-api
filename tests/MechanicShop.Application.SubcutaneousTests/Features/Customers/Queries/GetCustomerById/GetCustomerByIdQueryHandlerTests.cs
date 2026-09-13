using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.Customers.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomerById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetCustomerByIdQueryHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public GetCustomerByIdQueryHandlerTests(WebAppFactory factory)
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

        var query = new GetCustomerByIdQuery(Guid.NewGuid());

        var result = await _mediator.Send(query, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.CustomerNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var expectedCustomer = await CustomerTestHelper.CreateValidCustomer(_context, ct);

        var query = new GetCustomerByIdQuery(expectedCustomer.Id);

        var result = await _mediator.Send(query, ct);

        Assert.True(result.IsSuccess);
        var dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(expectedCustomer.Id, dto.CustomerId);
        Assert.Equal(expectedCustomer.Name, dto.Name);
        Assert.Equal(expectedCustomer.Email, dto.Email);
        Assert.Equal(expectedCustomer.PhoneNumber, dto.PhoneNumber);
        Assert.Equal(expectedCustomer.Vehicles.Count(), dto.Vehicles.Count);
    }
}
