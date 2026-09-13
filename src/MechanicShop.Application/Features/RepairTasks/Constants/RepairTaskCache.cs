namespace MechanicShop.Application.Features.RepairTasks.Constants
{
    public static class RepairTaskCache
    {
        private const string BaseName = "repair-tasks";

        public const string Tag = BaseName;

        public const string AllKey = $"{BaseName}:all";
        public static string ByIdKey(Guid id) => ByIdKey(id.ToString());
        public static string ByIdKey(string id) => $"{BaseName}:{id}";
    }
}
