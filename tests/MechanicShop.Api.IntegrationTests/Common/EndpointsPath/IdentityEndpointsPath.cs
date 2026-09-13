namespace MechanicShop.Api.IntegrationTests.Common.EndpointsPath;

public static class IdentityEndpointsPath
{
    public const string Version = "1.0";

    public const string BasePath = $"v{Version}/identity";

    public const string GenerateToken = $"{BasePath}/tokens/generate";

    public const string RefreshToken = $"{BasePath}/tokens/refresh";

    public const string GetCurrentUserInfo = $"{BasePath}/me";
    public static string GetUserById(Guid id) => $"{BasePath}/users/{id}";
}
