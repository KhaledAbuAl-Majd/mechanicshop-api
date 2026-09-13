using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Api.IntegrationTests.Common.EndpointsPath;
using MechanicShop.Api.Requests.V1.Customers;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Customers.Dtos;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CustomersControllerTests : IAsyncLifetime
{
    private readonly IAppDbContext _context;
    private readonly AppHttpClient _client;
    private readonly WebAppFactory _factory;

    public CustomersControllerTests(WebAppFactory factory)
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
    public async Task GetCustomers_ShouldReturnPaginatedList_WhenValidData()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        int page = 1, pageSize = 10;
        var requestUri = $"{CustomerEndpointsPath.GetCustomers}?Page={page}&PageSize={pageSize}";

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await _client.ReadFromJsonAsync<PaginatedList<CustomerListItemDto>>(response, ct);
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
    public async Task GetCustomers_ShouldReturnBadRequest_WhenPaginationInvalid(int page, int pageSize)
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = $"{CustomerEndpointsPath.GetCustomers}?Page={page}&PageSize={pageSize}";

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task GetCustomers_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);

        int page = 1, pageSize = 10;
        var requestUri = $"{CustomerEndpointsPath.GetCustomers}?Page={page}&PageSize={pageSize}";

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerById_ShouldReturnCustomer_WhenValidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var customer = await _context.SeedCustomerAsync(ct);

        var requestUri = CustomerEndpointsPath.GetCustomerById(customer.Id);

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await _client.ReadFromJsonAsync<CustomerDto>(response, ct);
        Assert.NotNull(dto);
        Assert.Equal(customer.Id, dto.CustomerId);
    }

    [Fact]
    public async Task GetCustomerById_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = CustomerEndpointsPath.GetCustomerById(Guid.NewGuid());

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task GetCustomerById_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);

        var requestUri = CustomerEndpointsPath.GetCustomerById(Guid.NewGuid());

        var response = await _client.GetAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    [Fact]
    public async Task CreateCustomer_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = CustomerEndpointsPath.CreateCustomer;

        var email = Guid.NewGuid().ToString()[..15] + "@gmail.com";
        var phoneNumber = "+" + string.Join("", Enumerable.Range(1, 11).Select(_ => Random.Shared.Next(1, 10)));
        var vehicle = new CreateVehicleRequest("bmw", "m5", 2025, $"p-{Guid.NewGuid().ToString()[..8]}");

        var request = new CreateCustomerRequest("khaled", PhoneNumber: phoneNumber, Email: email, Vehicles: [vehicle]);

        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var dto = await _client.ReadFromJsonAsync<CustomerDto>(response, ct);

        Assert.NotNull(dto);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task CreateCustomer_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);

        var requestUri = CustomerEndpointsPath.CreateCustomer;

        var email = Guid.NewGuid().ToString()[..15] + "@gmail.com";
        var phoneNumber = "+" + string.Join("", Enumerable.Range(1, 11).Select(_ => Random.Shared.Next(1, 10)));
        var vehicle = new CreateVehicleRequest("bmw", "m5", 2025, $"p-{Guid.NewGuid().ToString()[..8]}");

        var request = new CreateCustomerRequest("khaled", PhoneNumber: phoneNumber, Email: email, Vehicles: [vehicle]);

        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_ShouldReturnForbidden_WithoutManagerRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = CustomerEndpointsPath.CreateCustomer;

        var email = Guid.NewGuid().ToString()[..15] + "@gmail.com";
        var phoneNumber = "+" + string.Join("", Enumerable.Range(1, 11).Select(_ => Random.Shared.Next(1, 10)));
        var vehicle = new CreateVehicleRequest("bmw", "m5", 2025, $"p-{Guid.NewGuid().ToString()[..8]}");

        var request = new CreateCustomerRequest("khaled", PhoneNumber: phoneNumber, Email: email, Vehicles: [vehicle]);

        var response = await _client.PostAsJsonAsync(requestUri, request, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }


    [Fact]
    public async Task UpdateCustomer_ShouldSuccess_WhenValidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var customer = await _context.SeedCustomerAsync(ct);
        var vehicle = customer.Vehicles.First();

        var requestUri = CustomerEndpointsPath.UpdateCustomer(customer.Id);

        var vehicleRequest = new UpdateVehicleRequest(vehicle.Id, vehicle.Make, "m4", 2025, vehicle.LicensePlate);
        var request = new UpdateCustomerRequest("ahmed", customer.PhoneNumber!, customer.Email!, [vehicleRequest]);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = CustomerEndpointsPath.UpdateCustomer(Guid.NewGuid());

        var vehicleRequest = new UpdateVehicleRequest(Guid.NewGuid(), "bmw", "m4", 2025, "p-12345");
        var request = new UpdateCustomerRequest("ahmed", "+12345678901", "test@test.com", [vehicleRequest]);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task UpdateCustomer_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);

        var requestUri = CustomerEndpointsPath.UpdateCustomer(Guid.NewGuid());

        var vehicleRequest = new UpdateVehicleRequest(Guid.NewGuid(), "bmw", "m4", 2025, "p-12345");
        var request = new UpdateCustomerRequest("ahmed", "+12345678901", "test@test.com", [vehicleRequest]);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    [Fact]
    public async Task UpdateCustomer_ShouldReturnForbidden_WithoutManagerRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = CustomerEndpointsPath.UpdateCustomer(Guid.NewGuid());

        var vehicleRequest = new UpdateVehicleRequest(Guid.NewGuid(), "bmw", "m4", 2025, "p-12345");
        var request = new UpdateCustomerRequest("ahmed", "+12345678901", "test@test.com", [vehicleRequest]);

        var response = await _client.PutAsJsonAsync(requestUri, request, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    //

    [Fact]
    public async Task DeleteCustomer_ShouldSuccess_WhenValidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var customer = await _context.SeedCustomerAsync(ct);

        var requestUri = CustomerEndpointsPath.RemoveCustomer(customer.Id);

        var response = await _client.DeleteAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = CustomerEndpointsPath.RemoveCustomer(Guid.NewGuid());

        var response = await _client.DeleteAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task DeleteCustomer_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);

        var requestUri = CustomerEndpointsPath.RemoveCustomer(Guid.NewGuid());

        var response = await _client.DeleteAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    [Fact]
    public async Task DeleteCustomer_ShouldReturnForbidden_WithoutManagerRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = CustomerEndpointsPath.RemoveCustomer(Guid.NewGuid());

        var response = await _client.DeleteAsync(requestUri, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }


    public static TheoryData<string?> GetInvalidAuthentication() => new TheoryData<string?>()
    {
        null,
        string.Empty,
        "invalid token"
    };
}