using Asp.Versioning;
using MechanicShop.Application.Features.Billing.Constants;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats;
using MechanicShop.Application.Features.WorkOrders.Constants;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers.V1
{
    [Route("v{version:apiVersion}/dashboard")]
    [ApiVersion("1.0")]
    [Authorize]
    [Tags("Dashboard")]
    public class DashboardController(ISender sender, TimeProvider datetime) : ApiController
    {
        [HttpGet("stats")]
        [ProducesResponseType(typeof(TodayWorkOrderStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Get daily work order stats")]
        [EndpointDescription("Retrieves work order metrics and financial summary for a specific date adjusted by time zone.")]
        [EndpointName("GetWorkOrderStats")]
        [OutputCache(VaryByHeaderNames = ["X-TimeZone"], VaryByQueryKeys = ["*"], Duration = 100, Tags = [WorkOrderCache.Tag, InvoiceCache.Tag])]
        public async Task<ActionResult<TodayWorkOrderStatsDto>> GetTodayStats([FromHeader(Name = "X-TimeZone")] string? tz, DateOnly? date, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(tz))
            {
                var error = Error.Validation(description: "Time Zone Required. Missing time zone in 'X-TimeZone' header");

                return Problem([error]);
            }

            if (!TimeZoneInfo.TryFindSystemTimeZoneById(tz, out var timeZone))
            {
                var error = Error.Validation(description: $"Invalid or unknown time zone: '{tz}'");

                return Problem([error]);
            }

            var scheduleDate = date ?? DateOnly.FromDateTime(datetime.GetUtcNow().UtcDateTime);

            var query = new GetWorkOrderStatsQuery(timeZone, scheduleDate);

            var result = await sender.Send(query, ct);

            return result.Match(Ok, Problem);
        }

    }

}
