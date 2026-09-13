using System.Security.Claims;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Identity.Interfaces
{
    public interface ITokenProvider
    {
        Task<Result<TokenDto>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default);
        Result<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token);
    }
}
