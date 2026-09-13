namespace MechanicShop.Api.Requests.V1.Identity
{
    public record GenerateTokenRequest(string Email, string Password);
    public record RefreshTokenRequest(string RefreshToken, string ExpiredAccessToken);
}
