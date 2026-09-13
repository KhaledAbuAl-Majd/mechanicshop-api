using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Extensions;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MechanicShop.Infrastructure.Identity
{
    public class IdentityService(
        UserManager<AppUser> userManager,
        IUserClaimsPrincipalFactory<AppUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService) : IIdentityService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IUserClaimsPrincipalFactory<AppUser> _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        private readonly IAuthorizationService _authorizationService = authorizationService;

        public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null || !await _userManager.CheckPasswordAsync(user,password))
            {
                return Error.Unauthorized("Authentication.Failed", "Invalid email or password.");
            }

            if (!user.EmailConfirmed)
            {
                return Error.Forbidden("Email.NotConfirmed", $"Email '{email.MaskEmail()}' is not confirmed.");
            }


            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return new AppUserDto(
                user.Id,
                user.Email!,
                roles,
                claims);
        }

        public async Task<bool> AuthorizeAsync(string userId, string? policyName, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return false;
            }

            var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

            var result = await _authorizationService.AuthorizeAsync(principal, policyName!);

            return result.Succeeded;
        }

        public async Task<Result<AppUserDto>> GetUserByIdAsync(string userId, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return Error.NotFound(ApplicationErrors.UserNotFound.Code, $"User with id {userId} not found.");

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return new AppUserDto(
                user.Id,
                user.Email!,
                roles,
                claims);
        }

        public async Task<string?> GetUserNameAsync(string userId, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);

            return user?.UserName;
        }

        public async Task<bool> IsInRoleAsync(string userId, string role, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return false;

            return await _userManager.IsInRoleAsync(user, role);
        }
    }
}
