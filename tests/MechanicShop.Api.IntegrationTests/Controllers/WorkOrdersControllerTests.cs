using System.Net;
using System.Net.Http.Json;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Api.IntegrationTests.Common.EndpointsPath;
using MechanicShop.Api.Requests.V1.WorkOrders;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Security;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class WorkOrdersControllerTests : IAsyncLifetime
{
    private readonly IAppDbContext _context;
    private readonly AppHttpClient _client;
    private readonly WebAppFactory _factory;

    public WorkOrdersControllerTests(WebAppFactory factory)
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
    public async Task GetWorkOrders_ShouldReturnPaginagedList_WhenValidPagination()
    {
        var ct = CancellationToken.None;

        int page = 1;
        int pageSize = 10;
        var requestUri = $"{WorkOrderEndpointsPath.GetWorkOrders}?page={page}&pageSize={pageSize}";

        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PaginatedList<WorkOrderListItemDto>>(ct);

        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Equal(page, result.Page);
        Assert.Equal(pageSize, result.PageSize);
    }


    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    public async Task GetWorkOrders_ShouldReturnBadRequest_WhenPaginationInalid(int page, int pageSize)
    {
        var ct = CancellationToken.None;

        var requestUri = $"{WorkOrderEndpointsPath.GetWorkOrders}?page={page}&pageSize={pageSize}";

        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    [Fact]
    public async Task GetWorkOrders_ShouldApplyFiltersCorrectly_WhenValidFilters()
    {
        var ct = CancellationToken.None;

        int page = 1;
        int pageSize = 10;
        var vehicleId = Guid.NewGuid();
        var laborId = Guid.NewGuid();
        const string searchTerm = "test";
        const int state = (int)WorkOrderState.InProgress;
        const int spot = (int)Spot.A;
        var startDateFrom = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var startDateTo = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var queryString = $"page={page}&pageSize={pageSize}&searchTerm={searchTerm}&state={state}&vehicleId={vehicleId}&" +
            $"laborId={laborId}&spot={spot}&startDateFrom={startDateFrom}&startDateTo={startDateTo}";

        var requestUri = $"{WorkOrderEndpointsPath.GetWorkOrders}?{queryString}";

        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PaginatedList<WorkOrderListItemDto>>(ct);

        Assert.NotNull(result);
    }


    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task GetWorkOrders_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;

        _client.SetAuthorizationHeader(authentication!);

        int page = 1;
        int pageSize = 10;
        var requestUri = $"{WorkOrderEndpointsPath.GetWorkOrders}?page={page}&pageSize={pageSize}";

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    [Fact]
    public async Task GetWorkOrderById_ShouldReturnWorkOrder_WhenValidId()
    {
        var ct = CancellationToken.None;

        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);


        var repairTask = await _context.SeedRepairTaskAsync(ct);
        var customer = await _context.SeedCustomerAsync(ct);

        var workOrder = WorkOrderTestDataBuilder.Create()
            .WithRepairTasks(repairTask)
            .WithVehicle(customer.Vehicles.First().Id)
            .WithLabor(TestUsers.Labor01.Id)
            .Build();

        _context.WorkOrders.Add(workOrder);

        var saveResult = await _context.SaveChangesAsync(ct);

        var requestUri = WorkOrderEndpointsPath.GetWorkOrderById(workOrder.Id);

        var responose = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.OK, responose.StatusCode);

        var result = await _client.ReadFromJsonAsync<WorkOrderDto>(responose, ct);

        Assert.NotNull(result);
        Assert.Equal(workOrder.Id, result.WorkOrderId);
    }

    [Fact]
    public async Task GetWorkOrderById_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;

        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = WorkOrderEndpointsPath.GetWorkOrderById(Guid.NewGuid());

        var responose = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.NotFound, responose.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task GetWorkOrderById_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;

        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(authentication!);

        var requestUri = WorkOrderEndpointsPath.GetWorkOrderById(Guid.NewGuid());

        var responose = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, responose.StatusCode);
    }


    [Fact]
    public async Task CreateWorkOrder_ShouldSuccess_WhenValidRequest()
    {
        var ct = CancellationToken.None;
        string requestUri = WorkOrderEndpointsPath.CreateWorkOrder;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var customer = await _context.SeedCustomerAsync(ct);
        var repariTask = await _context.SeedRepairTaskAsync(ct);
        var laborId = Guid.Parse(TestUsers.Labor01.Id);

        var startAt = WorkOrderTestDataBuilder.GetOpening(DateTimeOffset.UtcNow.AddDays(2).DateTime);

        var request = new CreateWorkOrderRequest(
            Spot: Spot.B,
            VehicleId: customer.Vehicles.First().Id,
            StartAtUtc: startAt,
            RepairTaskIds: [repariTask.Id],
            LaborId: laborId);


        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var dto = await _client.ReadFromJsonAsync<WorkOrderDto>(response, ct);

        Assert.NotNull(dto);
    }


    [Fact]
    public async Task CreateWorkOrder_ShouldReturnBadRequest_WhenInvalidRequest()
    {
        var ct = CancellationToken.None;
        string requestUri = WorkOrderEndpointsPath.CreateWorkOrder;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var request = new CreateWorkOrderRequest(
            Spot: Spot.B,
            VehicleId: Guid.Empty,
            StartAtUtc: default,
            RepairTaskIds: [],
            LaborId: Guid.Empty);


        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task CreateWorkOrder_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        string requestUri = WorkOrderEndpointsPath.CreateWorkOrder;

        _client.SetAuthorizationHeader(authentication!);

        var startAt = WorkOrderTestDataBuilder.GetOpening(DateTimeOffset.UtcNow.AddDays(1).DateTime);

        var request = new CreateWorkOrderRequest(
            Spot: Spot.B,
            VehicleId: Guid.Empty,
            StartAtUtc: startAt,
            RepairTaskIds: [],
            LaborId: Guid.Empty);


        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkOrder_ShouldReturnForbidden_WithoutManagerRole()
    {
        var ct = CancellationToken.None;
        string requestUri = WorkOrderEndpointsPath.CreateWorkOrder;
        var token = await _client.GenerateLaborTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken!);

        var startAt = WorkOrderTestDataBuilder.GetOpening(DateTimeOffset.UtcNow.AddDays(1).DateTime);

        var request = new CreateWorkOrderRequest(
            Spot: Spot.B,
            VehicleId: Guid.Empty,
            StartAtUtc: startAt,
            RepairTaskIds: [],
            LaborId: Guid.Empty);


        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }


    [Fact]
    public async Task RelocateWorkOrder_ShouldUpdateWorkOrder_WhenValidRequest()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var customer = await _context.SeedCustomerAsync(ct);
        var repairTask = await _context.SeedRepairTaskAsync(ct);
        var laborId = Guid.Parse(TestUsers.Labor02.Id);

        var workOrder = WorkOrderTestDataBuilder.Create()
            .UpdateDate(hoursOffset: 3)
            .WithRepairTasks(repairTask)
            .WithVehicle(customer.Vehicles.First().Id)
            .WithLabor(laborId)
            .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(ct);

        var requestUri = WorkOrderEndpointsPath.RelocateWorkOrder(workOrder.Id);

        var request = new RelocateWorkOrderRequest(
            NewStartAtUtc: WorkOrderTestDataBuilder.GetOpening(DateTime.UtcNow.AddDays(3)),
            NewSpot: Spot.C);


        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RelocateWorkOrder_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = WorkOrderEndpointsPath.RelocateWorkOrder(Guid.NewGuid());

        var request = new RelocateWorkOrderRequest(
            NewStartAtUtc: WorkOrderTestDataBuilder.GetOpening(DateTime.UtcNow.AddDays(3)),
            NewSpot: Spot.C);


        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task RelocateWorkOrder_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;

        _client.SetAuthorizationHeader(authentication!);

        var requestUri = WorkOrderEndpointsPath.RelocateWorkOrder(Guid.NewGuid());

        var request = new RelocateWorkOrderRequest(
            NewStartAtUtc: WorkOrderTestDataBuilder.GetOpening(DateTime.UtcNow.AddDays(3)),
            NewSpot: Spot.C);


        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RelocateWorkOrder_ShouldReturnForbidden_WithoutManagerRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = WorkOrderEndpointsPath.RelocateWorkOrder(Guid.NewGuid());

        var request = new RelocateWorkOrderRequest(
            NewStartAtUtc: WorkOrderTestDataBuilder.GetOpening(DateTime.UtcNow.AddDays(3)),
            NewSpot: Spot.C);


        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AssignLabor_ShouldUpdateWorkOrder_WhenValidRequest()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var customer = await _context.SeedCustomerAsync(ct);
        var repairTask = await _context.SeedRepairTaskAsync(ct);
        var laborId = Guid.Parse(TestUsers.Labor02.Id);

        var workOrder = WorkOrderTestDataBuilder.Create()
            .UpdateDate(hoursOffset: 4)
            .WithRepairTasks(repairTask)
            .WithVehicle(customer.Vehicles.First().Id)
            .WithLabor(laborId)
            .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(ct);

        var requestUri = WorkOrderEndpointsPath.AssignLaborToWorkOrder(workOrder.Id);

        var request = new AssignLaborRequest(LaborId: Guid.Parse(TestUsers.Labor03.Id));

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task AssignLabor_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = WorkOrderEndpointsPath.AssignLaborToWorkOrder(Guid.NewGuid());

        var request = new AssignLaborRequest(LaborId: Guid.Parse(TestUsers.Labor03.Id));

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AssignLabor_ShouldReturnBadRequest_WhenInvalidLaborIdId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = WorkOrderEndpointsPath.AssignLaborToWorkOrder(Guid.NewGuid());

        var request = new AssignLaborRequest(LaborId: Guid.Empty);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task AssignLabor_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;

        _client.SetAuthorizationHeader(authentication!);

        var requestUri = WorkOrderEndpointsPath.AssignLaborToWorkOrder(Guid.NewGuid());

        var request = new AssignLaborRequest(LaborId: Guid.Parse(TestUsers.Labor03.Id));

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AssignLabor_ShouldReturnForbidden_WithoutManagerRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = WorkOrderEndpointsPath.AssignLaborToWorkOrder(Guid.NewGuid());

        var request = new AssignLaborRequest(LaborId: Guid.Parse(TestUsers.Labor03.Id));

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }


    [Fact]
    public async Task UpdateWorkOrderState_ShouldUpdateWorkOrder_WhenValidRequest()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var customer = await _context.SeedCustomerAsync(ct);
        var repairTask = await _context.SeedRepairTaskAsync(ct);
        var laborId = Guid.Parse(TestUsers.Labor02.Id);

        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(8));

        var workOrder = WorkOrderTestDataBuilder.Create(_factory.FakeTimeProvider)
            .UpdateDate(hoursOffset: 0)
            .WithRepairTasks(repairTask)
            .WithVehicle(customer.Vehicles.First().Id)
            .WithLabor(laborId)
            .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(ct);

        _factory.FakeTimeProvider.SetUtcNow(workOrder.StartAtUtc.AddMinutes(10));

        var requestUri = WorkOrderEndpointsPath.UpdateWorkOrderState(workOrder.Id);

        var request = new UpdateWorkOrderStateRequest(WorkOrderState.InProgress);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWorkOrderState_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = WorkOrderEndpointsPath.UpdateWorkOrderState(Guid.NewGuid());

        var request = new UpdateWorkOrderStateRequest(WorkOrderState.InProgress);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task UpdateWorkOrderState_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;

        _client.SetAuthorizationHeader(authentication!);

        var requestUri = WorkOrderEndpointsPath.UpdateWorkOrderState(Guid.NewGuid());

        var request = new UpdateWorkOrderStateRequest(WorkOrderState.InProgress);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWorkOrderState_ShouldUpdateWorkOrder_WhenSelfScopedLabor()
    {
        var ct = CancellationToken.None;
        var labor = TestUsers.Labor03;
        var token = await _client.GenerateTokenAsync(labor, ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var customer = await _context.SeedCustomerAsync(ct);
        var repairTask = await _context.SeedRepairTaskAsync(ct);
        var laborId = Guid.Parse(labor.Id);

        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(8));

        var workOrder = WorkOrderTestDataBuilder.Create(_factory.FakeTimeProvider)
            .UpdateDate(hoursOffset: 2)
            .WithRepairTasks(repairTask)
            .WithVehicle(customer.Vehicles.First().Id)
            .WithLabor(laborId)
            .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(ct);

        _factory.FakeTimeProvider.SetUtcNow(workOrder.StartAtUtc.AddMinutes(10));

        var requestUri = WorkOrderEndpointsPath.UpdateWorkOrderState(workOrder.Id);

        var request = new UpdateWorkOrderStateRequest(WorkOrderState.InProgress);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWorkOrderState_ShouldReturnForbidden_WithoudSelfScopedLabor()
    {
        var ct = CancellationToken.None;
        var labor = TestUsers.Labor03;
        var token = await _client.GenerateTokenAsync(labor, ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var customer = await _context.SeedCustomerAsync(ct);
        var repairTask = await _context.SeedRepairTaskAsync(ct);
        var laborId = Guid.Parse(TestUsers.Labor04.Id);


        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(8));

        var workOrder = WorkOrderTestDataBuilder.Create(_factory.FakeTimeProvider)
            .UpdateDate(hoursOffset: 5)
            .WithRepairTasks(repairTask)
            .WithVehicle(customer.Vehicles.First().Id)
            .WithLabor(laborId)
            .Build();
        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(ct);

        var requestUri = WorkOrderEndpointsPath.UpdateWorkOrderState(workOrder.Id);

        var request = new UpdateWorkOrderStateRequest(WorkOrderState.InProgress);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRepairTasks_ShouldUpdateWorkOrder_WhenValidRequest()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var customer = await _context.SeedCustomerAsync(ct);
        var repairTask = await _context.SeedRepairTaskAsync(ct);
        var laborId = Guid.Parse(TestUsers.Labor02.Id);

        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(9));

        var workOrder = WorkOrderTestDataBuilder.Create(_factory.FakeTimeProvider)
            .UpdateDate(hoursOffset: 0)
            .WithRepairTasks(repairTask)
            .WithVehicle(customer.Vehicles.First().Id)
            .WithLabor(laborId)
            .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(ct);

        var requestUri = WorkOrderEndpointsPath.UpdateWorkOrderRepairTasks(workOrder.Id);

        var newRepairTask = await _context.SeedRepairTaskAsync(ct);

        var request = new UpdateWorkOrderRepairTasksRequest([newRepairTask.Id]);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRepairTasks_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = WorkOrderEndpointsPath.UpdateWorkOrderRepairTasks(Guid.NewGuid());

        var request = new UpdateWorkOrderRepairTasksRequest([Guid.NewGuid()]);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task UpdateRepairTasks_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;

        _client.SetAuthorizationHeader(authentication!);

        var requestUri = WorkOrderEndpointsPath.UpdateWorkOrderRepairTasks(Guid.NewGuid());

        var request = new UpdateWorkOrderRepairTasksRequest([Guid.NewGuid()]);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRepairTasks_ShouldReturnForbidden_WithoutManagerRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = WorkOrderEndpointsPath.UpdateWorkOrderRepairTasks(Guid.NewGuid());

        var request = new UpdateWorkOrderRepairTasksRequest([Guid.NewGuid()]);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    //
    [Fact]
    public async Task DeleteWorkOrder_ShouldSuccess_WhenValidRequest()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var customer = await _context.SeedCustomerAsync(ct);
        var repairTask = await _context.SeedRepairTaskAsync(ct);
        var laborId = Guid.Parse(TestUsers.Labor02.Id);

        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(10));

        var workOrder = WorkOrderTestDataBuilder.Create(_factory.FakeTimeProvider)
            .UpdateDate(hoursOffset: 0)
            .WithRepairTasks(repairTask)
            .WithVehicle(customer.Vehicles.First().Id)
            .WithLabor(laborId)
            .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(ct);

        var requestUri = WorkOrderEndpointsPath.DeleteWorkOrder(workOrder.Id);

        var response = await _client.DeleteAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteWorkOrder_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = WorkOrderEndpointsPath.DeleteWorkOrder(Guid.NewGuid());

        var response = await _client.DeleteAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task DeleteWorkOrder_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;

        _client.SetAuthorizationHeader(authentication!);

        var requestUri = WorkOrderEndpointsPath.DeleteWorkOrder(Guid.NewGuid());

        var response = await _client.DeleteAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteWorkOrder_ShouldReturnForbidden_WithoutManagerRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);

        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = WorkOrderEndpointsPath.DeleteWorkOrder(Guid.NewGuid());

        var response = await _client.DeleteAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    //

    [Fact]
    public async Task GetSchedule_ShouldApplyFiltersCorrectly_WhenValidFilters()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var customer = await _context.SeedCustomerAsync(ct);
        var repairTask = await _context.SeedRepairTaskAsync(ct);
        var laborId = Guid.Parse(TestUsers.Labor02.Id);

        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(11));

        var workOrder = WorkOrderTestDataBuilder.Create(_factory.FakeTimeProvider)
            .UpdateDate(hoursOffset: 0)
            .WithRepairTasks(repairTask)
            .WithVehicle(customer.Vehicles.First().Id)
            .WithLabor(laborId)
            .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(ct);


        var requestUri = WorkOrderEndpointsPath.GetDailySchedule(DateOnly.FromDateTime(_factory.FakeTimeProvider.GetUtcNow().DateTime));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        var tz = _factory.FakeTimeProvider.LocalTimeZone;
        request.Headers.Add("X-TimeZone", tz.Id);

        var response = await _client.SendAsync(request, ct: ct);

        var resultString = response.Content.ReadAsStringAsync(ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ScheduleDto>(AppHttpClient.JsonSerializerOptions, ct);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Spots);
    }


    [Fact]
    public async Task GetSchedule_ShouldApplyLaborIdFiltersCorrectly_WhenValidFilters()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var customer = await _context.SeedCustomerAsync(ct);
        var repairTask = await _context.SeedRepairTaskAsync(ct);
        var laborId = Guid.Parse(TestUsers.Labor02.Id);

        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(11));

        var workOrder = WorkOrderTestDataBuilder.Create(_factory.FakeTimeProvider)
            .UpdateDate(hoursOffset: 0)
            .WithRepairTasks(repairTask)
            .WithVehicle(customer.Vehicles.First().Id)
            .WithLabor(laborId)
            .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(ct);


        var requestUri = $"{WorkOrderEndpointsPath.GetDailySchedule(DateOnly.FromDateTime(_factory.FakeTimeProvider.GetUtcNow().DateTime))}?LaborId={workOrder.LaborId}";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        var tz = _factory.FakeTimeProvider.LocalTimeZone;
        request.Headers.Add("X-TimeZone", tz.Id);

        var response = await _client.SendAsync(request, ct: ct);

        var resultString = response.Content.ReadAsStringAsync(ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ScheduleDto>(AppHttpClient.JsonSerializerOptions, ct);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Spots);
    }


    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task GetSchedule_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;

        _client.SetAuthorizationHeader(authentication!);

        var requestUri = WorkOrderEndpointsPath.GetDailySchedule(DateOnly.FromDateTime(_factory.FakeTimeProvider.GetUtcNow().DateTime));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        var tz = _factory.FakeTimeProvider.LocalTimeZone;
        request.Headers.Add("X-TimeZone", tz.Id);

        var response = await _client.SendAsync(request, ct: ct);


        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    public static TheoryData<string?> GetInvalidAuthentication() => new TheoryData<string?>()
    {
        null,
        string.Empty,
        "invalid token"
    };
}
