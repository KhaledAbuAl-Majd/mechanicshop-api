namespace MechanicShop.Api.Responses.V1.Identity
{
    public sealed record AppUserResponse(
      string UserId,
      string Email,
      IList<string> Roles
  );
}
