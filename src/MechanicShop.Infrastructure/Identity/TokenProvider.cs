using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Settings;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Interfaces;
using MechanicShop.Domain.Common.Results;
using Microsoft.IdentityModel.Tokens;

namespace MechanicShop.Infrastructure.Identity
{
    public class TokenProvider(JwtSettings jwtSettings, TimeProvider datetime) : ITokenProvider
    {
        private readonly JwtSettings _jwtSettings = jwtSettings;
        private readonly TimeProvider _datetime = datetime;

        public async Task<Result<TokenDto>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default)
        {
            return await CreateAsync(user, ct);
        }

        public Result<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return ApplicationErrors.ExpiredAccessTokenInvalid;
            }

            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
            {
                return ApplicationErrors.ExpiredAccessTokenInvalid;

                //return Error.Validation("AccessToken.invalid", "Access token is invalid");
            }

            //validate token manual (same as authentication middleware)

            var tokenValidatorParamters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = false,//that's for expired token also
                ClockSkew = TimeSpan.Zero
            };



            var principal = tokenHandler.ValidateToken(token, tokenValidatorParamters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return ApplicationErrors.ExpiredAccessTokenInvalid;
                //return Error.Validation("AccessToken.invalid", "Access token is invalid");
            }

            if (jwtSecurityToken.ValidTo > _datetime.GetUtcNow())
            {
                return ApplicationErrors.AccessTokenNotExpired;
            }

            return principal;
        }

        public async Task<Result<TokenDto>> CreateAsync(AppUserDto user, CancellationToken ct = default)
        {
            var expires = _datetime.GetUtcNow().AddMinutes(_jwtSettings.TokenExpirationInMinutes);

            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub,user.UserId!),
                new (JwtRegisteredClaimNames.Email,user.Email),
            };

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires.UtcDateTime,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var securityToken = tokenHandler.CreateToken(descriptor);

            var generatedRefreshToken = GenerateRefreshToken();


            return new TokenDto(tokenHandler.WriteToken(securityToken), generatedRefreshToken, expires);
        }

        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
