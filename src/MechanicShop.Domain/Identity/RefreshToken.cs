using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Identity
{
    public sealed class RefreshToken : AuditableEntity
    {
        public string TokenHash { get; private set; } = null!;
        public string UserId { get; private set; } = null!;
        public DateTimeOffset ExpiresOnUtc { get; private set; }
        public bool IsRevoked { get; private set; }
        public DateTimeOffset? RevokedAt { get; private set; } = null;

        public bool IsActive(TimeProvider datetime) =>
             !IsRevoked && ExpiresOnUtc >= datetime.GetUtcNow();

        private RefreshToken() { }

        private RefreshToken(Guid id, string token, string userId, DateTimeOffset expiresOnUtc, bool isRevoked, DateTimeOffset? revokedAt) : base(id)
        {
            TokenHash = token;
            UserId = userId;
            ExpiresOnUtc = expiresOnUtc;
            IsRevoked = isRevoked;
            RevokedAt = revokedAt;
        }

        public static Result<RefreshToken> Create(Guid id, string tokenHash, string userId, DateTimeOffset expiresOnUtc, TimeProvider datetime)
        {
            if (id == Guid.Empty)
                return RefreshTokenErrors.IdRequired;

            if (string.IsNullOrWhiteSpace(tokenHash))
                return RefreshTokenErrors.TokenRequired;

            if (string.IsNullOrWhiteSpace(userId))
                return RefreshTokenErrors.UserIdRequired;

            if (expiresOnUtc <= datetime.GetUtcNow())
                return RefreshTokenErrors.ExpiryInvalid;

            return new RefreshToken(id, tokenHash, userId, expiresOnUtc, false, null);
        }

        public Result<Updated> Revoke(TimeProvider datetime)
        {
            if (IsRevoked)
                return RefreshTokenErrors.RefreshTokenAlreadyRevoked;

            IsRevoked = true;
            RevokedAt = datetime.GetUtcNow();

            return Result.Updated;
        }
    }
}
