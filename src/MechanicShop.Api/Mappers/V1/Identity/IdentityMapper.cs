using MechanicShop.Api.Responses.V1.Identity;
using MechanicShop.Application.Features.Identity.Dtos;

namespace MechanicShop.Api.Mappers.V1.Identity
{
    public static class IdentityMapper
    {
        public static AppUserResponse ToResponse(this AppUserDto dto)
        {
            return new AppUserResponse(dto.UserId, dto.Email, dto.Roles);
        }
    }
}
