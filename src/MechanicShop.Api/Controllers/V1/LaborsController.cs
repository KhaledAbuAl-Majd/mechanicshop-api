using Asp.Versioning;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Features.Identity.Constants;
using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Application.Features.Labors.Queries.GetLabors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers.V1
{
    [Route("v{version:apiVersion}/labors")]
    [ApiVersion("1.0")]
    [Authorize]
    public class LaborsController(ISender sender) : ApiController
    {
        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)]
        [ProducesResponseType(typeof(List<LaborDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Retrieves the list of available labor definitions.")]
        [EndpointDescription("Returns all labor records associated with the system, accessible only to users with the Manager role.")]
        [EndpointName("GetLabors")]
        [OutputCache(Duration = 60, Tags = [UserCache.Tag])]
        public async Task<ActionResult<List<LaborDto>>> Get(CancellationToken ct)
        {
            var query = new GetLaborsQuery();

            var result = await sender.Send(query, ct);

            return result.Match(Ok, Problem);
        }
    }
}
