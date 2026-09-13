using Asp.Versioning;
using MechanicShop.Api.Filters.Idempotency;
using MechanicShop.Api.Requests.V1;
using MechanicShop.Api.Requests.V1.Customers;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Application.Features.Customers.Constants;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Application.Features.Customers.Queries.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers.V1
{
    [Route("v{version:apiVersion}/customers")]
    [ApiVersion("1.0")]
    [Authorize]
    [Tags("Customers")]
    public class CustomersController(ISender sender, IOutputCacheStore cache) : ApiController
    {
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<CustomerListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        [EndpointSummary("Retrieves a paginated list of customers.")]
        [EndpointDescription("Returns all customers paginated.")]
        [EndpointName("GetCustomers")]
        [OutputCache(VaryByQueryKeys = ["*"], Duration = 60, Tags = [CustomerCache.Tag])]
        public async Task<ActionResult<PaginatedList<CustomerListItemDto>>> Get([FromQuery] PageRequest pageRequest, CancellationToken ct)
        {
            var query = new GetCustomersQuery(pageRequest.Page, pageRequest.PageSize);

            var result = await sender.Send(query, ct);

            return result.Match(Ok, Problem);
        }

        [HttpGet("{id}", Name = "GetCustomerById")]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        [EndpointSummary("Retrieves a customer by ID.")]
        [EndpointDescription("Returns detailed information about the specified customer if found.")]
        [EndpointName("GetCustomerById")]
        [OutputCache(VaryByRouteValueNames = ["id"], Duration = 60, Tags = [CustomerCache.Tag])]
        public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken ct)
        {
            var query = new GetCustomerByIdQuery(id);

            var result = await sender.Send(query, ct);



            return result.Match(Ok, Problem);
        }


        [HttpPost]
        [Idempotent]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Creates a new customer.")]
        [EndpointDescription("Adds a new customer to the system.")]
        [EndpointName("CreateCustomer")]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken ct)
        {
            var command = new CreateCustomerCommand(
                request.Name,
                request.PhoneNumber,
                request.Email,
                request.Vehicles.ConvertAll(v => new CreateVehicleCommand(v.Make, v.Model, v.Year, v.LicensePlate)));

            var result = await sender.Send(command, ct);


            if (result.IsSuccess)
            {
                await cache.EvictByTagAsync(CustomerCache.Tag, ct);
            }

            return result.Match(
                response => CreatedAtRoute(
                    "GetCustomerById",
                    new { version = "1.0", id = response.CustomerId },
                    response),
                Problem);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Updates an existing customer.")]
        [EndpointDescription("Updates a customer and its associated vehicle.")]
        [EndpointName("")]
        public async Task<IActionResult> Update([FromBody] UpdateCustomerRequest request, Guid id, CancellationToken ct)
        {
            var command = new UpdateCustomerCommand(
                id,
                request.Name,
                request.PhoneNumber,
                request.Email,
                request.Vehicles.ConvertAll(v => new UpdateVehicleCommand(v.VehicleId, v.Make, v.Model, v.Year, v.LicensePlate)));

            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
            {
                await cache.EvictByTagAsync(CustomerCache.Tag, ct);
            }

            return result.Match(_ => NoContent(), Problem);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Removes a customer.")]
        [EndpointDescription("Deletes the specified customer from the system.")]
        [EndpointName("RemoveCustomer")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var command = new RemoveCustomerCommand(id);

            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
            {
                await cache.EvictByTagAsync(CustomerCache.Tag, ct);
            }

            return result.Match(_ => NoContent(), Problem);
        }
    }
}
