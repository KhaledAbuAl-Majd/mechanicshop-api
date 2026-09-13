namespace MechanicShop.Api.IntegrationTests.Common.EndpointsPath;

public static class CustomerEndpointsPath
{
    public const string Version = "1.0";

    public const string BasePath = $"v{Version}/customers";

    public const string GetCustomers = $"{BasePath}";
    public static string GetCustomerById(Guid id) => $"{BasePath}/{id}";

    public const string CreateCustomer = $"{BasePath}";
    public static string UpdateCustomer(Guid id) => $"{BasePath}/{id}";
    public static string RemoveCustomer(Guid id) => $"{BasePath}/{id}";

}