using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> IsInRoleAsync(string userId, string role, CancellationToken ct = default);
        Task<bool> AuthorizeAsync(string userId, string? policyName, CancellationToken ct = default);
        Task<Result<AppUserDto>> AuthenticateAsync(string email, string password, CancellationToken ct = default);//login 
        Task<Result<AppUserDto>> GetUserByIdAsync(string userId, CancellationToken ct = default);
        Task<string?> GetUserNameAsync(string userId, CancellationToken ct = default);
    }
}
