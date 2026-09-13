using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Identity
{
    public static class RefreshTokenErrors
    {
        public static Error IdRequired =>
            Error.Validation("RefreshToken.Id.Required", "Refresh token ID is required.");

        public static Error TokenRequired =>
            Error.Validation("RefreshToken.TokenHash.Required", "TokenHash value is required.");

        public static Error UserIdRequired =>
            Error.Validation("RefreshToken.UserId.Required", "User ID is required.");

        public static Error ExpiryInvalid =>
            Error.Validation("RefreshToken.Expiry.Invalid", "Expiry must be in the future.");

        public static Error RefreshTokenAlreadyRevoked =>
            Error.Conflict("RefreshToken.Already.Revoked", "Refresh TokenHash is already revoked.");
    }
}
