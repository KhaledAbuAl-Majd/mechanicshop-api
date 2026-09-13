using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Settings;
using MechanicShop.Application.Common.Utilities;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Commands.GenerateToken
{
    public sealed class GenerateTokenCommandHandler(
        ILogger<GenerateTokenCommandHandler> logger,
        IIdentityService identityService,
        ITokenProvider tokenProvider,
        IAppDbContext context,
        TimeProvider datetime,
        JwtSettings jwtSettings) : IRequestHandler<GenerateTokenCommand, Result<TokenDto>>
    {
        private readonly ILogger<GenerateTokenCommandHandler> _logger = logger;
        private readonly IIdentityService _identityService = identityService;
        private readonly ITokenProvider _tokenProvider = tokenProvider;
        private readonly IAppDbContext _context = context;
        private readonly TimeProvider _datetime = datetime;
        private readonly JwtSettings _jwtSettings = jwtSettings;

        public async Task<Result<TokenDto>> Handle(GenerateTokenCommand command, CancellationToken ct)
        {
            //Allow Multi-Device Login

            var userResponse = await _identityService.AuthenticateAsync(command.Email, command.Password, ct);

            if (userResponse.IsError)
            {
                _logger.LogWarning("User Authentication error occured: {@Errors}", userResponse.Errors);

                return userResponse.Errors;
            }

            var generateTokenResult = await _tokenProvider.GenerateJwtTokenAsync(userResponse.Value, ct);

            if (generateTokenResult.IsError)
            {
                _logger.LogError("Generate token error occured: {@Errors}", generateTokenResult.Errors);

                return generateTokenResult.Errors;
            }

            var generatedTokenDto = generateTokenResult.Value;

            var userId = userResponse.Value.UserId;

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

            _logger.LogInformation("Token generated Successfully for user Id {id}", userId);

            return generatedTokenDto;
        }
    }
}
