namespace MechanicShop.Application.Common.Constants
{
    public static class AuthorizationPolicies
    {
        public const string ManagerOnly = "ManagerOnly";
        public const string SelfScopedWorkOrderAccess = "SelfScopedWorkOrderAccess";
        public const string UserOwnerOrManager = "UserOwnerOrManager";
    }
}
