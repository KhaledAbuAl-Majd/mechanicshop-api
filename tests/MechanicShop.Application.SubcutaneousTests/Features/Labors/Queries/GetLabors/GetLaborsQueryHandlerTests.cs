using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labors.Queries.GetLabors;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity.Enums;
using MechanicShop.Tests.Common.Security;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.Labors.Queries.GetLabors;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetLaborsQueryHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public GetLaborsQueryHandlerTests(WebAppFactory factory)
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
        var ct = CancellationToken.None;

        List<Employee> employees = [
                    Employee.Create(Guid.Parse(TestUsers.Manager.Id), "Primary", "Manager", Role.Manager).Value,
                    Employee.Create(Guid.Parse(TestUsers.Labor01.Id), "John", "S.", Role.Labor).Value,
                    Employee.Create(Guid.Parse(TestUsers.Labor02.Id), "Peter", "R.", Role.Labor).Value,
                    Employee.Create(Guid.Parse(TestUsers.Labor03.Id), "Kevin", "M.", Role.Labor).Value,
                    Employee.Create(Guid.Parse(TestUsers.Labor04.Id), "Suzan", "L.", Role.Labor).Value
                ];

        _context.Employees.AddRange(employees);

        await _context.SaveChangesAsync(ct);

        var laborsCount = employees.Where(e => e.Role == Role.Labor).Count();

        var query = new GetLaborsQuery();

        var result = await _mediator.Send(query, ct);

        Assert.True(result.IsSuccess);
        var laborsDto = result.Value;
        Assert.NotNull(laborsDto);
        Assert.True(laborsDto.Count >= laborsCount);
    }
}
