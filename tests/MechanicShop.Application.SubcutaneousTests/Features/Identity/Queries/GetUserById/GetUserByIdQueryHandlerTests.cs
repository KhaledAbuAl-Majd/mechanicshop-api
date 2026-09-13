using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Queries.GetUserById;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Security;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.GetUserById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetUserByIdQueryHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public GetUserByIdQueryHandlerTests(WebAppFactory factory)
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

        var query = new GetUserByIdQuery(Guid.NewGuid().ToString());

        var result = await _mediator.Send(query, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.UserNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var expectedUser = TestUsers.Manager;

        var query = new GetUserByIdQuery(expectedUser.Id.ToString());

        var result = await _mediator.Send(query, ct);

        Assert.True(result.IsSuccess);
        var dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(expectedUser.Id, dto.UserId);
        Assert.Equal(expectedUser.Email, dto.Email);
    }
}
