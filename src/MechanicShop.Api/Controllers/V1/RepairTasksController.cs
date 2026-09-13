using Asp.Versioning;
using MechanicShop.Api.Filters.Idempotency;
using MechanicShop.Api.Requests.V1.RepairTasks;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Application.Features.RepairTasks.Constants;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers.V1
{
    [Route("v{version:apiVersion}/repair-tasks")]
    [ApiVersion("1.0")]
    [Authorize]
    [Tags("Repair Tasks")]
    public class RepairTasksController(ISender sender, IOutputCacheStore cache) : ApiController
    {
        [HttpGet]
        [ProducesResponseType(typeof(List<RepairTaskDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        [EndpointSummary("Retrieves all repair tasks.")]
        [EndpointDescription("Returns a list of all repair tasks available in the system.")]
        [EndpointName("GetRepairTasks")]
        [OutputCache(Duration = 60, Tags = [RepairTaskCache.Tag])]
        public async Task<ActionResult<List<RepairTaskDto>>> Get(CancellationToken ct)
        {
            var query = new GetRepairTasksQuery();

            var result = await sender.Send(query, ct);

            return result.Match(Ok, Problem);
        }

        [HttpGet("{id}", Name = "GetRepairTaskById")]
        [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        [EndpointSummary("Retrieves a repair task by ID.")]
        [EndpointDescription("Returns detailed information for the specified repair task if it exists.")]
        [EndpointName("GetRepairTaskById")]
        [OutputCache(VaryByRouteValueNames = ["id"], Duration = 60, Tags = [RepairTaskCache.Tag])]
        public async Task<ActionResult<RepairTaskDto>> GetById(Guid id, CancellationToken ct)
        {
            var query = new GetRepairTaskByIdQuery(id);

            var result = await sender.Send(query, ct);

            return result.Match(Ok, Problem);
        }

        [HttpPost]
        [Idempotent]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)]
        [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Creates a new repair task.")]
        [EndpointDescription("Creates a repair task and optionally includes parts.")]
        [EndpointName("CreateRepairTask")]
        public async Task<ActionResult<RepairTaskDto>> Create([FromBody] CreateRepairTaskRequest request, CancellationToken ct)
        {
            var command = new CreateRepairTaskCommand(request.Name,
                request.EstimatedDurationInMins,
                request.LaborCost,
                request.Parts.ConvertAll(p => new CreateRepairTaskPartCommand(p.Name, p.Cost, p.Quantity)));

            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
                await InvalidateOutputCacheAsync(ct);

            return result.Match(
                response => CreatedAtRoute(
                    routeName: "GetRepairTaskById",
                    routeValues: new { version = "1.0", id = response.RepairTaskId },
                    value: response),
                Problem);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Updates an existing repair task.")]
        [EndpointDescription("Updates a repair task and its associated parts.")]
        [EndpointName("UpdateRepairTask")]
        public async Task<IActionResult> Update([FromBody] UpdateRepairTaskRequest request, Guid id, CancellationToken ct)
        {
            var command = new UpdateRepairTaskCommand(
                id,
                request.Name,
                request.LaborCost,
                request.EstimatedDurationInMins,
                request.Parts.ConvertAll(p => new UpdateRepairTaskPartCommand(p.PartId, p.Name, p.Cost, p.Quantity)));

            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
                await InvalidateOutputCacheAsync(ct);

            return result.Match(_ => NoContent(), Problem);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Removes a repair task.")]
        [EndpointDescription("Deletes the specified repair task from the system.")]
        [EndpointName("RemoveRepairTask")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var command = new RemoveRepairTaskCommand(id);

            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
                await InvalidateOutputCacheAsync(ct);

            return result.Match(_ => NoContent(), Problem);
        }

        private async Task InvalidateOutputCacheAsync(CancellationToken ct)
        {
            await cache.EvictByTagAsync(RepairTaskCache.Tag, ct);
        }
    }
}
