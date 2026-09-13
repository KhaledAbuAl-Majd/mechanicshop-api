using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Api.IntegrationTests.Common.EndpointsPath;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class InvoicesControllerTests : IAsyncLifetime
{
    private readonly IAppDbContext _context;
    private readonly AppHttpClient _client;
    private readonly WebAppFactory _factory;

    public InvoicesControllerTests(WebAppFactory factory)
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
    public async Task IssueInvoice_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(1));

        var workOrder = await _context.SeedWorkOrderAsync(
            provider: _factory.FakeTimeProvider,
            state: WorkOrderState.Completed,
            ct: ct);

        var requestUri = InvoiceEndpointsPath.IssueInvoiceForWorkOrder(workOrder.Id);

        var response = await _client.PostAsync(requestUri, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var dto = await _client.ReadFromJsonAsync<InvoiceDto>(response, ct);
        Assert.NotNull(dto);
        Assert.Equal(workOrder.Id, dto.WorkOrderId);
    }

    [Fact]
    public async Task IssueInvoice_ShouldReturnNotFound_WhenInvalidWorkOrderId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = InvoiceEndpointsPath.IssueInvoiceForWorkOrder(Guid.NewGuid());

        var response = await _client.PostAsync(requestUri, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task IssueInvoice_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);

        var requestUri = InvoiceEndpointsPath.IssueInvoiceForWorkOrder(Guid.NewGuid());

        var response = await _client.PostAsync(requestUri, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task IssueInvoice_ShouldReturnForbidden_WithoutManagerRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = InvoiceEndpointsPath.IssueInvoiceForWorkOrder(Guid.NewGuid());

        var response = await _client.PostAsync(requestUri, attachIdempotencyKey: true, ct: ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    //

    [Fact]
    public async Task GetInvoiceById_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(2));

        var invoice = await _context.SeedInvoiceAsync(
            provider: _factory.FakeTimeProvider,
            ct: ct);

        var requestUri = InvoiceEndpointsPath.GetInvoiceById(invoice.Id);

        var response = await _client.GetAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await _client.ReadFromJsonAsync<InvoiceDto>(response, ct);
        Assert.NotNull(dto);
        Assert.Equal(invoice.Id, dto.InvoiceId);
    }

    [Fact]
    public async Task GetInvoiceById_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = InvoiceEndpointsPath.GetInvoiceById(Guid.NewGuid());

        var response = await _client.GetAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task GetInvoiceById_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);

        var requestUri = InvoiceEndpointsPath.GetInvoiceById(Guid.NewGuid());

        var response = await _client.GetAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetInvoiceById_ShouldReturnForbidden_WithoutMangarRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = InvoiceEndpointsPath.GetInvoiceById(Guid.NewGuid());

        var response = await _client.GetAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }


    [Fact]
    public async Task GetInvoiceByPdfId_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(3));

        var invoice = await _context.SeedInvoiceAsync(
            provider: _factory.FakeTimeProvider,
            ct: ct);

        var requestUri = InvoiceEndpointsPath.GetInvoicePdfById(invoice.Id);

        var response = await _client.GetAsync(requestUri, ct: ct);

        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.PartialContent);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains(".pdf", response.Content.Headers.ContentDisposition?.FileName);

        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public async Task GetInvoicePdfById_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = InvoiceEndpointsPath.GetInvoicePdfById(Guid.NewGuid());

        var response = await _client.GetAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task GetInvoicePdfById_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);

        var requestUri = InvoiceEndpointsPath.GetInvoicePdfById(Guid.NewGuid());

        var response = await _client.GetAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    [Fact]
    public async Task GetInvoicePdfById_ShouldReturnForbidden_WithoutMangarRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = InvoiceEndpointsPath.GetInvoicePdfById(Guid.NewGuid());

        var response = await _client.GetAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }


    //

    [Fact]
    public async Task SettleInvoice_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(5));

        var invoice = await _context.SeedInvoiceAsync(
            provider: _factory.FakeTimeProvider,
            ct: ct);

        var requestUri = InvoiceEndpointsPath.SettleInvoiceById(invoice.Id);

        var response = await _client.PutAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);;
    }

    [Fact]
    public async Task SettleInvoice_ShouldReturnNotFound_WhenInvalidId()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateManagerTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = InvoiceEndpointsPath.SettleInvoiceById(Guid.NewGuid());

        var response = await _client.PutAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidAuthentication))]
    public async Task SettleInvoice_ShouldReturnUnauthorized_WithoutAuthentication(string? authentication)
    {
        var ct = CancellationToken.None;
        _client.SetAuthorizationHeader(authentication!);

        var requestUri = InvoiceEndpointsPath.SettleInvoiceById(Guid.NewGuid());

        var response = await _client.PutAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SettleInvoice_ShouldReturnForbidden_WithoutMangarRole()
    {
        var ct = CancellationToken.None;
        var token = await _client.GenerateLaborTokenAsync(ct);
        _client.SetAuthorizationHeader(token.AccessToken);

        var requestUri = InvoiceEndpointsPath.SettleInvoiceById(Guid.NewGuid());

        var response = await _client.PutAsync(requestUri, ct: ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }


    public static TheoryData<string?> GetInvalidAuthentication() => new TheoryData<string?>()
    {
        null,
        string.Empty,
        "invalid token"
    };
}
