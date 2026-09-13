using System.Security.Claims;
using MechanicShop.Domain.Identity.Enums;
using Microsoft.AspNetCore.Authorization;

namespace MechanicShop.Infrastructure.Identity.Polices
{
    public class UserOwnerOrManagerRequirement : IAuthorizationRequirement;

    public class UserOwnerOrManagerHandler : AuthorizationHandler<UserOwnerOrManagerRequirement, Guid>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserOwnerOrManagerRequirement requirement, Guid resource)
        {
            if (context.User.IsInRole(nameof(Role.Manager)))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is not null && userId.Equals(resource.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
