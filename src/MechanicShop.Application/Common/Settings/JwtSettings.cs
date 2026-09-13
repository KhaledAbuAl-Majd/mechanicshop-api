namespace MechanicShop.Application.Common.Settings
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public string Secret { get; init; } = string.Empty;
        public int TokenExpirationInMinutes { get; init; }
        public int RefreshTokenExpirationInDays { get; init; } = 7;
    }
}
