using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Identity.Commands.GenerateToken
{
    public sealed record GenerateTokenCommand(string Email, string Password) : IRequest<Result<TokenDto>>;
}
