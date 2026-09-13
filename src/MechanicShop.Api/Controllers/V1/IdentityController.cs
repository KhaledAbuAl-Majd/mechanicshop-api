using System.Security.Claims;
using Asp.Versioning;
using MechanicShop.Api.Mappers.V1.Identity;
using MechanicShop.Api.Requests.V1.Identity;
using MechanicShop.Api.Responses.V1.Identity;
using MechanicShop.Api.Settings;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Features.Identity.Commands.GenerateToken;
using MechanicShop.Application.Features.Identity.Commands.RefreshToken;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MechanicShop.Api.Controllers.V1
{
    [Route("v{version:apiVersion}/identity")]
    [ApiVersion("1.0")]
    //[ApiVersionNeutral] 
    [Tags("Identity")]
    public class IdentityController(ISender sender) : ApiController
    {
        [HttpPost("tokens/generate")]
        [EnableRateLimiting(AuthRateLimiterOptions.PolicyName)]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Generates an access and refresh token for a valid user.")]
        [EndpointDescription("Authenticates a user using provided credentials and returns a JWT token pair.")]
        [EndpointName("GenerateToken")]
        public async Task<ActionResult<TokenDto>> GenerateToken([FromBody] GenerateTokenRequest request, CancellationToken ct)
        {
            var command = new GenerateTokenCommand(request.Email, request.Password);

            var result = await sender.Send(command, ct);

            return result.Match(Ok, Problem);
        }

        [HttpPost("tokens/refresh")]
        [EnableRateLimiting(AuthRateLimiterOptions.PolicyName)]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Refreshes access token using a valid refresh token.")]
        [EndpointDescription("Exchanges an expired access token and a valid refresh token for a new token pair.")]
        [EndpointName("RefreshToken")]
        public async Task<ActionResult<TokenDto>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            var command = new RefreshTokenCommand(request.RefreshToken, request.ExpiredAccessToken);

            var result = await sender.Send(command, ct);

            return result.Match(Ok, Problem);
        }

        [HttpGet("me")]
        [Authorize]//logged in user - authenticated
        [ProducesResponseType(typeof(AppUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [EndpointSummary("Gets the current authenticated user's info.")]
        [EndpointDescription("Returns user information for the currently authenticated user based on the access token.")]
        [EndpointName("GetCurrentUserInfo")]
        public async Task<ActionResult<AppUserResponse>> GetCurrentUserInfo(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = new GetUserByIdQuery(userId!);

            var result = await sender.Send(query, ct);

            return result.Match(response => Ok(response.ToResponse()), Problem);
        }

        [HttpGet("users/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(AppUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [EndpointSummary("Gets user info by id.")]
        [EndpointDescription("Returns user information for specified user id, access allowed only for user self or manager.")]
        [EndpointName("GetUserInfoById")]
        public async Task<ActionResult<AppUserResponse>> GetUserById([FromServices] IAuthorizationService authorizationService, Guid id, CancellationToken ct)
        {
            var authorizeResult = await authorizationService.AuthorizeAsync(User, id, AuthorizationPolicies.UserOwnerOrManager);

            if (!authorizeResult.Succeeded)
            {
                var error = Domain.Common.Results.Error.Forbidden(description: "Access allowed only for user self or manager.");

                return Problem([error]);
            }

            var query = new GetUserByIdQuery(id.ToString());

            var result = await sender.Send(query, ct);

            return result.Match(response => Ok(response.ToResponse()), Problem);
        }
    }
}
