using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserById
{
    public sealed class GetUserByIdQueryHandler(ILogger<GetUserByIdQueryHandler> logger, IIdentityService identityService) : IRequestHandler<GetUserByIdQuery, Result<AppUserDto>>
    {
        private readonly ILogger<GetUserByIdQueryHandler> _logger = logger;
        private readonly IIdentityService _identityService = identityService;

        public async Task<Result<AppUserDto>> Handle(GetUserByIdQuery query, CancellationToken ct)
        {
            var getUserByIdResult = await _identityService.GetUserByIdAsync(query.UserId, ct);

            if (getUserByIdResult.IsError)
            {
                _logger.LogWarning("User with Id {UserId} {ErrorDetails}", query.UserId, getUserByIdResult.TopError.Description);

                return getUserByIdResult.Errors;
            }

            return getUserByIdResult.Value;
        }
    }
}
