namespace MechanicShop.Application.Features.Identity.Constants
{
    public static class UserCache
    {
        private const string BaseName = "users";

        public const string Tag = BaseName;

        public const string AllKey = $"{BaseName}:all";
        public static string ByIdKey(Guid id) => ByIdKey(id.ToString());
        public static string ByIdKey(string id) => $"{BaseName}:{id}";
    }
}
