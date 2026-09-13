namespace MechanicShop.Application.Features.Customers.Constants
{
    public static class CustomerCache
    {
        private const string BaseName = "customers";

        public const string Tag = BaseName;

        public const string AllKey = $"{BaseName}:all";
        public static string ByIdKey(Guid id) => ByIdKey(id.ToString());
        public static string ByIdKey(string id) => $"{BaseName}:{id}";
    }
}
