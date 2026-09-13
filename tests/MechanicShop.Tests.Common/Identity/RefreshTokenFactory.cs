using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;

namespace MechanicShop.Tests.Common.Identity;

public static class RefreshTokenFactory
{
    public static Result<RefreshToken> CreateRefreshToken(
        Guid? id = null,
        string? tokenHash = "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08",
        string? userId = "33d4f395-d3ba-424c-b21d-2daf1f0547f3",
        DateTimeOffset? expiresOnUtc = null,
        TimeProvider? provider = null)
    {
        return RefreshToken.Create(
            id ?? Guid.NewGuid(),
            tokenHash!,
            userId!,
            expiresOnUtc ?? DateTimeOffset.UtcNow.AddDays(7),
            provider ?? TimeProvider.System);
    }
}
