using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Queries.GetCustomers;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.Customers.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetCustomersQueryHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public GetCustomersQueryHandlerTests(WebAppFactory factory)
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

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Handle_ShouldReturnPaginatedAndProjectedData(int pageNumber)
    {
        var ct = CancellationToken.None;

        var customer1 = await CustomerTestHelper.CreateValidCustomer(_context, ct);
        var customer2 = await CustomerTestHelper.CreateValidCustomer(_context, ct);
        var customer3 = await CustomerTestHelper.CreateValidCustomer(_context, ct);

        int totalCount = 3;
        int pageSize = 2;
        bool hasNextPage = (pageSize * pageNumber) < totalCount;
        bool hasPreviousPage = pageNumber > 1;
        int offset = (pageNumber - 1) * pageSize;
        int currentPageCount = hasNextPage ? pageSize : totalCount - offset;


        var query = new GetCustomersQuery(pageNumber, pageSize);

        var result = await _mediator.Send(query, ct);

        Assert.True(result.IsSuccess);

        var resultValue = result.Value;
        Assert.NotNull(resultValue);
        Assert.Equal(query.Page, resultValue.Page);
        Assert.Equal(query.PageSize, resultValue.PageSize);
        Assert.Equal(totalCount, resultValue.TotalCount);
        Assert.Equal(hasNextPage, resultValue.HasNextPage);
        Assert.Equal(hasPreviousPage, resultValue.HasPreviousPage);
        Assert.Equal(currentPageCount, resultValue.Items.Count);
    }
}
