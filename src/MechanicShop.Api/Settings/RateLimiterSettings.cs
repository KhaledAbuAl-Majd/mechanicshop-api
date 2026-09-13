namespace MechanicShop.Api.Settings;

public sealed class RateLimiterSettings
{
    public const string SectionName = "RateLimiterSettings";

    public GlobalRateLimiterOptions Global { get; init; } = new();
    public AuthRateLimiterOptions Auth { get; init; } = new();
    public ConcurrencyRateLimiterOptions HeavyExport { get; init; } = new();
}

public sealed class GlobalRateLimiterOptions
{
    public int PermitLimit { get; init; } = 60;
    public int QueueLimit { get; init; } = 10;
    public int WindowInMinutes { get; init; } = 1;
    public int SegmentsPerWindow { get; init; } = 6;
}

public sealed class AuthRateLimiterOptions
{
    public const string PolicyName = "AuthRateLimiter";
    public int PermitLimit { get; init; } = 5;
    public int QueueLimit { get; init; } = 0;
    public int WindowInMinutes { get; init; } = 1;
    public int SegmentsPerWindow { get; init; } = 6;

}

public sealed class ConcurrencyRateLimiterOptions
{
    public const string PolicyName = "ConcurrencyRateLimiter";
    public int PermitLimit { get; init; } = 3;
    public int QueueLimit { get; init; } = 0;
}