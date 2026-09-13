namespace MechanicShop.Application.Features.Identity.Dtos
{
    public sealed record TokenDto(
        string AccessToken,
        string RefreshToken,
        DateTimeOffset ExpiresOnUtc);
}
