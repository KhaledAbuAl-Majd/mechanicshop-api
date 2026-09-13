namespace MechanicShop.Api.IntegrationTests.Common.EndpointsPath;

public static class DashboardEndpointsPath
{
    public const string Version = "1.0";

    public const string BasePath = $"v{Version}/dashboard";

    public const string GetWorkOrderStats = $"{BasePath}/stats";
}