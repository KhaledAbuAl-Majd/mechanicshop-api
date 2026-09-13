using Asp.Versioning;
using MechanicShop.Api.Filters.Idempotency;
using MechanicShop.Api.Settings;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;
using MechanicShop.Application.Features.Billing.Constants;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;
using MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace MechanicShop.Api.Controllers.V1
{
    [Route("v{version:apiVersion}/invoices")]
    [ApiVersion("1.0")]
    [Authorize(AuthorizationPolicies.ManagerOnly)]
    [Tags("Invoices")]
    public class InvoicesController(ISender sender, IOutputCacheStore cache) : ApiController
    {

        [HttpPost("work-orders/{workOrderId}")]
        [Idempotent]
        [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Issues an invoice for a work order.")]
        [EndpointDescription("Creates a new invoice for the specified work order and returns the created invoice resource.")]
        [EndpointName("IssueInvoiceForWorkOrder")]

        public async Task<ActionResult<InvoiceDto>> IssueInvoice(Guid workOrderId, CancellationToken ct)
        {
            var command = new IssueInvoiceCommand(workOrderId);

            var result = await sender.Send(command, ct);

            return result.Match(
                response => CreatedAtRoute(
                    routeName: "GetInvoiceById",
                    routeValues: new { version = "1.0", id = response.InvoiceId },
                    value: response),
                Problem);
        }


        [HttpGet("{id}", Name = "GetInvoiceById")]
        [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Retrieves an invoice by ID.")]
        [EndpointDescription("Returns detailed information about the specified invoice. Only users with the Manager role are authorized.")]
        [EndpointName("GetInvoiceById")]
        [OutputCache(VaryByRouteValueNames = ["id"], Duration = 60, Tags = [InvoiceCache.Tag])]
        public async Task<ActionResult<InvoiceDto>> GetById(Guid id, CancellationToken ct)
        {
            var query = new GetInvoiceByIdQuery(id);

            var result = await sender.Send(query, ct);

            return result.Match(Ok, Problem);
        }

        [HttpGet("{id}/pdf", Name = "GetInvoicePdfById")]
        [EnableRateLimiting(ConcurrencyRateLimiterOptions.PolicyName)]
        [ProducesResponseType(typeof(FileContentResult),StatusCodes.Status200OK, "application/pdf")]
        [ProducesResponseType(typeof(FileContentResult),StatusCodes.Status206PartialContent, "application/pdf")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Downloads the invoice as a PDF file.")]
        [EndpointDescription("Returns the invoice PDF file for the specified invoice ID. Only users with the Manager role are authorized.")]
        [EndpointName("GetInvoicePdfById")]
        [OutputCache(VaryByRouteValueNames = ["id"], Duration = 60, Tags = [InvoiceCache.Tag])]
        public async Task<IActionResult> GetPdfById(Guid id, CancellationToken ct)
        {
            var query = new GetInvoicePdfQuery(id);

            var result = await sender.Send(query, ct);

            return result.Match(response => File(response.Content!, response.ContentType!, response.FileName), Problem);
        }

        [HttpPut("{id}/payments")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Marks an invoice as paid.")]
        [EndpointDescription("Settles the specified invoice. Only users with the Manager role are authorized to perform this operation.")]
        [EndpointName("SettleInvoice")]
        public async Task<IActionResult> SettleInvoice(Guid id, CancellationToken ct)
        {
            var command = new SettleInvoiceCommand(id);

            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
                await InvalidateOutputCacheAsync(ct);

            return result.Match(_ => NoContent(), Problem);
        }

        private async Task InvalidateOutputCacheAsync(CancellationToken ct)
        {
            await cache.EvictByTagAsync(InvoiceCache.Tag, ct);
        }
    }
}
