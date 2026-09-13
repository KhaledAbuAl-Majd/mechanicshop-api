using System.Security.Claims;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Settings;
using MechanicShop.Application.Common.Utilities;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandHandler(
        ILogger<RefreshTokenCommandHandler> logger,
        IIdentityService identityService,
        IAppDbContext context,
        ITokenProvider tokenProvider,
        TimeProvider datetime,
        JwtSettings jwtSettings) : IRequestHandler<RefreshTokenCommand, Result<TokenDto>>
    {
        private readonly ILogger<RefreshTokenCommandHandler> _logger = logger;
        private readonly IIdentityService _identityService = identityService;
        private readonly IAppDbContext _context = context;
        private readonly ITokenProvider _tokenProvider = tokenProvider;
        private readonly TimeProvider _datetime = datetime;
        private readonly JwtSettings _jwtSettings = jwtSettings;

        public async Task<Result<TokenDto>> Handle(RefreshTokenCommand command, CancellationToken ct)
        {
            var getPrincipalResult = _tokenProvider.GetPrincipalFromExpiredToken(command.ExpiredAccessToken);

            if (getPrincipalResult.IsError)
            {
                _logger.LogWarning("Get principal from token failed, errors: {@Errors}", getPrincipalResult.Errors);

                return getPrincipalResult.Errors;
            }

            var principal = getPrincipalResult.Value;

            if (principal is null)
            {
                _logger.LogWarning("Expired acces token is not valid");

                return ApplicationErrors.ExpiredAccessTokenInvalid;
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId is null)
            {
                _logger.LogWarning("Invalid userId claim");

                return ApplicationErrors.UserIdClaimInvalid;
            }

            var getUserResult = await _identityService.GetUserByIdAsync(userId, ct);

            if (getUserResult.IsError)
            {
                _logger.LogWarning("Get user by id error occurred: {@Errors}", getUserResult.Errors);
                return getUserResult.Errors;
            }

            var hashedRefreshToken = HashHelper.ComputeSha256(command.RefreshToken);

            var oldRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(
                rt => rt.TokenHash == hashedRefreshToken && rt.UserId == userId,
                ct);

            if (oldRefreshToken is null || !oldRefreshToken.IsActive(_datetime))

            {
                _logger.LogWarning("Refresh token is invalid or expired for user {UserId}", userId);

                return ApplicationErrors.RefreshTokenInvalidOrExpired;
            }

            var generateTokenResult = await _tokenProvider.GenerateJwtTokenAsync(getUserResult.Value, ct);

            if (generateTokenResult.IsError)
            {
                _logger.LogError("Generate token error occurred: {@Errors}", generateTokenResult.Errors);

                return generateTokenResult.Errors;
            }

            var generatedTokenDto = generateTokenResult.Value;

            var revokeTokenResult = oldRefreshToken.Revoke(_datetime);

            if (revokeTokenResult.IsError)
            {
                _logger.LogWarning("Revoke token with Id {@tokenId}, error occured: {@Errors}", oldRefreshToken.Id, revokeTokenResult.Errors);

                return revokeTokenResult.Errors;
            }

            var refreshTokenExpires = _datetime.GetUtcNow().AddDays(_jwtSettings.RefreshTokenExpirationInDays);

            var createRefreshTokenResult = Domain.Identity.RefreshToken.Create(
                Guid.NewGuid(),
                HashHelper.ComputeSha256(generatedTokenDto.RefreshToken),
                userId,
                refreshTokenExpires,
                _datetime);

            if (createRefreshTokenResult.IsError)
            {
                _logger.LogWarning("Create refresh token failed, error occured: {@Errors}", createRefreshTokenResult.Errors);
                return createRefreshTokenResult.Errors;
            }

            var newRefreshToken = createRefreshTokenResult.Value;

            _context.RefreshTokens.Add(newRefreshToken);

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Refresh Token Successfully for user Id {id}", userId);

            return generatedTokenDto;
        }
    }
}
