using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Api.IntegrationTests.Common.EndpointsPath;
using MechanicShop.Api.Requests.V1.RepairTasks;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RepairTasksControllerTests : IAsyncLifetime
{
    private readonly IAppDbContext _context;
    private readonly AppHttpClient _client;
    private readonly WebAppFactory _factory;

    public RepairTasksControllerTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAppHttpClient();
        _context = factory.CreateAppDbContext();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }


    [Fact]
    public async Task GetRepairTasks_ShouldReturnRepairTaskList_WhenValidData()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);
        var requestUri = RepairTaskEndpointsPath.GetRepairTasks;


        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await _client.ReadFromJsonAsync<List<RepairTaskDto>>(response, ct);

        Assert.NotNull(result);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task GetRepairTasks_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);
        var requestUri = RepairTaskEndpointsPath.GetRepairTasks;

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRepairTaskById_ShouldReturnRepairTask_WhenValidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var repairTask = await _context.SeedRepairTaskAsync(ct);

        var requestUri = RepairTaskEndpointsPath.GetRepairTaskById(repairTask.Id);

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await _client.ReadFromJsonAsync<RepairTaskDto>(response, ct);

        Assert.NotNull(dto);
        Assert.Equal(repairTask.Id, dto.RepairTaskId);
    }

    [Fact]
    public async Task GetRepairTaskById_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = RepairTaskEndpointsPath.GetRepairTaskById(Guid.NewGuid());

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task GetRepairTaskById_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);
        var requestUri = RepairTaskEndpointsPath.GetRepairTaskById(Guid.NewGuid());

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    [Fact]
    public async Task CreateRepairTask_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);
        var requestUri = RepairTaskEndpointsPath.CreateRepairTask;

        var partRequest = new CreateRepairTaskPartRequest(
            Name: $"part-{Guid.NewGuid().ToString()[..5]}",
            Cost: 3,
            Quantity: 4);

        var request = new CreateRepairTaskRequest(
            Name: "reapair task",
            EstimatedDurationInMins: RepairDurationInMinutes.Min45,
            LaborCost: 40,
            Parts: [partRequest]);

        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await _client.ReadFromJsonAsync<RepairTaskDto>(response, ct);

        Assert.NotNull(dto);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task CreateRepairTask_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);
        var requestUri = RepairTaskEndpointsPath.CreateRepairTask;

        var partRequest = new CreateRepairTaskPartRequest(
            Name: $"part-{Guid.NewGuid().ToString()[..5]}",
            Cost: 3,
            Quantity: 4);

        var request = new CreateRepairTaskRequest(
            Name: "reapair task",
            EstimatedDurationInMins: RepairDurationInMinutes.Min45,
            LaborCost: 40,
            Parts: [partRequest]);

        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRepairTask_ShouldReturnForbidden_WithoutManagerRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);
        var requestUri = RepairTaskEndpointsPath.CreateRepairTask;

        var partRequest = new CreateRepairTaskPartRequest(
            Name: $"part-{Guid.NewGuid().ToString()[..5]}",
            Cost: 3,
            Quantity: 4);

        var request = new CreateRepairTaskRequest(
            Name: "reapair task",
            EstimatedDurationInMins: RepairDurationInMinutes.Min45,
            LaborCost: 40,
            Parts: [partRequest]);

        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); ;
    }

    [Fact]
    public async Task UpdateRepairTask_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var repairTask = await _context.SeedRepairTaskAsync(ct);
        var part = repairTask.Parts.First();

        var partRequest = new UpdateRepairTaskPartRequest(
            PartId: part.Id,
            Name: part.Name!,
            Cost: 10,
            Quantity: 3);

        var request = new UpdateRepairTaskRequest(
            Name: "reapair task",
            EstimatedDurationInMins: RepairDurationInMinutes.Min60,
            LaborCost: 90,
            Parts: [partRequest]);

        var requestUri = RepairTaskEndpointsPath.UpdateRepairTask(repairTask.Id);
        var response = await _client.PutAsJsonAsync(requestUri, request, ct: ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRepairTask_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var partRequest = new UpdateRepairTaskPartRequest(
            PartId: Guid.NewGuid(),
            Name: "part name",
            Cost: 10,
            Quantity: 3);

        var request = new UpdateRepairTaskRequest(
            Name: "repair task",
            EstimatedDurationInMins: RepairDurationInMinutes.Min60,
            LaborCost: 90,
            Parts: [partRequest]);

        var requestUri = RepairTaskEndpointsPath.UpdateRepairTask(Guid.NewGuid());
        var response = await _client.PutAsJsonAsync(requestUri, request, ct: ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); ;
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task UpdateRepairTask_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);

        var partRequest = new UpdateRepairTaskPartRequest(
             PartId: Guid.NewGuid(),
             Name: "part name"!,
             Cost: 10,
             Quantity: 3);

        var request = new UpdateRepairTaskRequest(
            Name: "repair task",
            EstimatedDurationInMins: RepairDurationInMinutes.Min60,
            LaborCost: 90,
            Parts: [partRequest]);

        var requestUri = RepairTaskEndpointsPath.UpdateRepairTask(Guid.NewGuid());
        var response = await _client.PutAsJsonAsync(requestUri, request, ct: ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRepairTask_ShouldReturnForbidden_WithoutManagerRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var partRequest = new UpdateRepairTaskPartRequest(
            PartId: Guid.NewGuid(),
            Name: "part name"!,
            Cost: 10,
            Quantity: 3);

        var request = new UpdateRepairTaskRequest(
            Name: "repair task",
            EstimatedDurationInMins: RepairDurationInMinutes.Min60,
            LaborCost: 90,
            Parts: [partRequest]);

        var requestUri = RepairTaskEndpointsPath.UpdateRepairTask(Guid.NewGuid());
        var response = await _client.PutAsJsonAsync(requestUri, request, ct: ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); ;
    }

    //

    [Fact]
    public async Task DeleteRepairTask_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var repairTask = await _context.SeedRepairTaskAsync(ct);

        var requestUri = RepairTaskEndpointsPath.DeleteRepairTask(repairTask.Id);
        var response = await _client.DeleteAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRepairTask_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = RepairTaskEndpointsPath.DeleteRepairTask(Guid.NewGuid());
        var response = await _client.DeleteAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); ;
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task DeleteRepairTask_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);

        var requestUri = RepairTaskEndpointsPath.DeleteRepairTask(Guid.NewGuid());
        var response = await _client.DeleteAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRepairTask_ShouldReturnForbidden_WithoutManagerRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = RepairTaskEndpointsPath.DeleteRepairTask(Guid.NewGuid());
        var response = await _client.DeleteAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    public static TheoryData<string?> GetInvalidAuthentication() => new TheoryData<string?>()
    {
        null,
        string.Empty,
        "invalid token"
    };
}
