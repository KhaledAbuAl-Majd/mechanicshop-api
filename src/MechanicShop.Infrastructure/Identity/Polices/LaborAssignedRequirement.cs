using System.Security.Claims;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Identity.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Identity.Polices
{
    public class LaborAssignedRequirement : IAuthorizationRequirement;

    public class LaborAssignedHandler(IAppDbContext context) : AuthorizationHandler<LaborAssignedRequirement, Guid>
    {
        private readonly IAppDbContext _context = context;
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, LaborAssignedRequirement requirement, Guid workOrderId)
        {
            if (context.User.IsInRole(nameof(Role.Manager)))
            {
                context.Succeed(requirement);
                return;
            }

            var userIdString = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                //context.Fail();
                return;
            }


            var isAssigned = await _context.WorkOrders.AnyAsync(wo => wo.Id == workOrderId && wo.LaborId == userId);

            if (isAssigned)
            {
                context.Succeed(requirement);
                return;
            }

            //context.Fail();
        }
    }

    //another way using HttpContextAccess and route value

    //public class LaborAssignedHandler(IAppDbContext context, IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<LaborAssignedRequirement>
    //{
    //    private readonly IAppDbContext _context = context;
    //    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    //    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, LaborAssignedRequirement requirement)
    //    {
    //        if (context.User.IsInRole(nameof(Role.Manager)))
    //        {
    //            context.Succeed(requirement);
    //            return;
    //        }

    //        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    //        if (string.IsNullOrEmpty(userId))
    //        {
    //            context.Fail();
    //            return;
    //        }

    //        var workOrderIdString = _httpContextAccessor.HttpContext?.Request.RouteValues["WorkOrderId"]?.ToString();

    //        if (!Guid.TryParse(workOrderIdString, out var workOrderId))
    //        {
    //            context.Fail();
    //            return;
    //        }

    //        var isAssigned = await _context.WorkOrders.AnyAsync(wo => wo.Id == workOrderId && wo.LaborId == Guid.Parse(userId));

    //        if (isAssigned)
    //        {
    //            context.Succeed(requirement);
    //            return;
    //        }

    //        context.Fail();
    //    }
    //}


}
