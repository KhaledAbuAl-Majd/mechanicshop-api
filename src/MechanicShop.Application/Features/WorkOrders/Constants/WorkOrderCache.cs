namespace MechanicShop.Application.Features.WorkOrders.Constants
{
    public static class WorkOrderCache
    {
        private const string BaseName = "work-orders";

        public const string Tag = BaseName;

        public const string AllKey = $"{BaseName}:all";
        public static string ByIdKey(Guid id) => ByIdKey(id.ToString());
        public static string ByIdKey(string id) => $"{BaseName}:{id}";
    }
}
