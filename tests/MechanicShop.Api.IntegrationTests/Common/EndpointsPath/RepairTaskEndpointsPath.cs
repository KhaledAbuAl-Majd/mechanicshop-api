namespace MechanicShop.Api.IntegrationTests.Common.EndpointsPath;

public static class RepairTaskEndpointsPath
{
    public const string Version = "1.0";

    public const string BasePath = $"v{Version}/repair-tasks";

    public const string GetRepairTasks = $"{BasePath}";
    public static string GetRepairTaskById(Guid id) => $"{BasePath}/{id}";

    public const string CreateRepairTask = $"{BasePath}";
    public static string UpdateRepairTask(Guid id) => $"{BasePath}/{id}";
    public static string DeleteRepairTask(Guid id) => $"{BasePath}/{id}";

}