namespace MechanicShop.Api.IntegrationTests.Common.EndpointsPath;

public static class WorkOrderEndpointsPath
{
    public const string Version = "1.0";

    public const string BasePath = $"v{Version}/work-orders";

    public const string GetWorkOrders = $"{BasePath}";
    public static string GetWorkOrderById(Guid id) => $"{BasePath}/{id}";

    public const string CreateWorkOrder = $"{BasePath}";
    public static string RelocateWorkOrder(Guid id) => $"{BasePath}/{id}/relocate";
    public static string AssignLaborToWorkOrder(Guid id) => $"{BasePath}/{id}/labor";
    public static string UpdateWorkOrderState(Guid id) => $"{BasePath}/{id}/state";
    public static string UpdateWorkOrderRepairTasks(Guid id) => $"{BasePath}/{id}/repair-tasks";
    public static string DeleteWorkOrder(Guid id) => $"{BasePath}/{id}";
    public static string GetDailySchedule(DateOnly? date = null) => $"{BasePath}/schedule/{date?.ToString("yyyy-MM-dd") ?? string.Empty}";
}
