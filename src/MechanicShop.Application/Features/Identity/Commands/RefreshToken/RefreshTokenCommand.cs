using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Identity.Commands.RefreshToken
{
    public sealed record RefreshTokenCommand(string RefreshToken, string ExpiredAccessToken) : IRequest<Result<TokenDto>>;
}
