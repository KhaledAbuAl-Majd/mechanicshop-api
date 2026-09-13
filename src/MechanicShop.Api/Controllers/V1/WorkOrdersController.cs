using Asp.Versioning;
using MechanicShop.Api.Filters.Idempotency;
using MechanicShop.Api.Requests.V1;
using MechanicShop.Api.Requests.V1.WorkOrders;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Application.Features.Scheduling.Queries.GetDailySchedule;
using MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;
using MechanicShop.Application.Features.WorkOrders.Constants;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers.V1
{
    [Route("v{version:apiVersion}/work-orders")]
    [ApiVersion("1.0")]
    [Authorize]
    [Tags("Work Orders")]
    public class WorkOrdersController(ISender sender, IOutputCacheStore cache, TimeProvider datetime) : ApiController
    {
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<WorkOrderListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Retrieves a paginated list of work orders.")]
        [EndpointDescription(
        "Supports filtering by date range, status, vehicle, labor, spot, and searching by term. Pagination and sorting are supported.")]
        [EndpointName("GetWorkOrders")]
        [OutputCache(VaryByQueryKeys = ["*"], Duration = 60, Tags = [WorkOrderCache.Tag])]
        public async Task<ActionResult<PaginatedList<WorkOrderListItemDto>>> Get(
            [FromQuery] WorkOrderFilterRequest filters,
            [FromQuery] PageRequest pageRequest,
            CancellationToken ct)
        {

            var query = new GetWorkOrdersQuery(
                Page: pageRequest.Page,
                PageSize: pageRequest.PageSize,
                SearchTerm: filters.SearchTerm,
                SortColumn: filters.SortColumn,
                SortDirection: filters.SortDirection,
                State: filters.State,
                VehicleId: filters.VehicleId,
                LaborId: filters.LaborId,
                StartDateFrom: filters.StartDateFrom,
                StartDateTo: filters.StartDateTo,
                EndDateFrom: filters.EndDateFrom,
                EndDateTo: filters.EndDateTo,
                Spot: filters.Spot);

            var result = await sender.Send(query, ct);

            return result.Match(Ok, Problem);
        }

        [HttpGet("{id}", Name = "GetWorkOrderById")]
        [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointDescription("Returns detailed information about the specified work order if it exists.")]
        [EndpointName("GetWorkOrderById")]
        [OutputCache(VaryByRouteValueNames = ["id"], Duration = 60, Tags = [WorkOrderCache.Tag])]
        public async Task<ActionResult<WorkOrderDto>> GetById(Guid id, CancellationToken ct)
        {
            var query = new GetWorkOrderByIdQuery(id);

            var result = await sender.Send(query, ct);

            return result.Match(Ok, Problem);
        }


        [HttpPost]
        [Idempotent]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)]
        [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Creates a new work order.")]
        [EndpointDescription("Creates a new work order for a vehicle, specifying labor, tasks, and other required information.")]
        [EndpointName("CreateWorkOrder")]
        public async Task<ActionResult<WorkOrderDto>> Create([FromBody] CreateWorkOrderRequest request, CancellationToken ct)
        {
            var command = new CreateWorkOrderCommand(request.Spot, request.VehicleId, request.StartAtUtc, request.RepairTaskIds, request.LaborId);

            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
                await InvalidateOutputCacheAsync(ct);

            return result.Match(
                response => CreatedAtRoute(
                    routeName: "GetWorkOrderById",
                    routeValues: new { version = "1.0", id = response.WorkOrderId },
                    value: response),
                Problem);
        }


        [HttpPut("{id}/relocate")]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Relocates a work order to a new time and spot.")]
        [EndpointDescription(
        "Updates the scheduled time and assigned bay for a work order. Only users with the Manager role can perform this action.")]
        [EndpointName("RelocateWorkOrder")]
        public async Task<IActionResult> Relocate([FromBody] RelocateWorkOrderRequest request, Guid id, CancellationToken ct)
        {
            var command = new RelocateWorkOrderCommand(id, request.NewStartAtUtc, request.NewSpot);

            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
                await InvalidateOutputCacheAsync(ct);

            return result.Match(_ => NoContent(), Problem);
        }


        [HttpPut("{id}/labor")]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Assigns a labor to a work order.")]
        [EndpointDescription(
        "Associates a labor definition with a specific work order. Only managers can perform this operation.")]
        [EndpointName("AssignLaborToWorkOrder")]
        public async Task<IActionResult> AssignLabor([FromBody] AssignLaborRequest request, Guid id, CancellationToken ct)
        {
            var command = new AssignLaborCommand(id, request.LaborId);

            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
                await InvalidateOutputCacheAsync(ct);

            return result.Match(_ => NoContent(), Problem);
        }


        [HttpPut("{id}/state")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Changes the state of a work order.")]
        [EndpointDescription(
        "Updates the current state of the specified work order. Only users with the Manager role and assigned labor are authorized.")]
        [EndpointName("UpdateWorkOrderState")]
        public async Task<IActionResult> UpdateState(
            [FromServices] IAuthorizationService authorizationService,
            [FromBody] UpdateWorkOrderStateRequest request,
            Guid id,
            CancellationToken ct)
        {
            var authorizeResult = await authorizationService.AuthorizeAsync(
                user: User,
                policyName: AuthorizationPolicies.SelfScopedWorkOrderAccess,
                resource: id);

            if (!authorizeResult.Succeeded)
            {
                var error = Error.Forbidden(description: "Only users with the Manager role and assigned labor are authorized.");

                return Problem([error]);
            }

            var command = new UpdateWorkOrderStateCommand(id, request.State);

            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
                await InvalidateOutputCacheAsync(ct);

            return result.Match(_ => NoContent(), Problem);
        }


        [HttpPut("{id}/repair-tasks")]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Modify work order repair tasks.")]
        [EndpointDescription("Update the repair tasks of the specified work order if found. Only users with the manager role are authorized.")]
        [EndpointName("UpdateWorkOrderRepairTasks")]
        public async Task<IActionResult> UpdateRepairTasks([FromBody] UpdateWorkOrderRepairTasksRequest request, Guid id, CancellationToken ct)
        {
            var command = new UpdateWorkOrderRepairTasksCommand(id, request.RepairTaskIds);

            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
                await InvalidateOutputCacheAsync(ct);

            return result.Match(_ => NoContent(), Problem);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointDescription("Deletes the specified work order permanently. Only users with the Manager role are authorized.")]
        [EndpointName("DeleteWorkOrder")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var command = new DeleteWorkOrderCommand(id);

            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
                await InvalidateOutputCacheAsync(ct);

            return result.Match(_ => NoContent(), Problem);
        }

        [HttpGet("schedule/{date?}")]
        [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Retrieves the schedule for a given day.")]
        [EndpointDescription(
        "Returns a schedule view for the specified date. If no date is provided, today's schedule is returned. You can optionally filter by labor ID.")]
        [EndpointName("GetDailySchedule")]
        [OutputCache(VaryByRouteValueNames = ["date"], VaryByHeaderNames = ["X-TimeZone"], VaryByQueryKeys = ["*"], Duration = 60, Tags = [WorkOrderCache.Tag])]
        public async Task<ActionResult<ScheduleDto>> GetSchedule(DateOnly? date,
            [FromQuery] GetDailyScheduleRequest request,
            [FromHeader(Name = "X-TimeZone")] string? tz,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(tz))
            {
                var error = Error.Validation(description: "Time Zone Required. Missing time zone in 'X-TimeZone' header");

                return Problem([error]);
            }

            TimeZoneInfo timeZone;

            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(tz);
            }
            catch
            {
                var error = Error.Validation(description: $"Invalid or unknown time zone: '{tz}'");

                return Problem([error]);
            }

            var scheduleDate = date ?? DateOnly.FromDateTime(datetime.GetUtcNow().UtcDateTime);


            var query = new GetDailyScheduleQuery(timeZone, scheduleDate, request.LaborId);

            var result = await sender.Send(query, ct);

            return result.Match(Ok, Problem);
        }

        private async Task InvalidateOutputCacheAsync(CancellationToken ct)
        {
            await cache.EvictByTagAsync(WorkOrderCache.Tag, ct);
        }
    }
}
