using MechanicShop.Application.Features.Identity.Commands.GenerateToken;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Tests.Common.Security;
using MediatR;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Common;

public static class IdentityTestHelper
{
    public static async Task<TokenDto> GenerateValidToken(IMediator mediator, string email, string password, CancellationToken ct = default)
    {
        var command = new GenerateTokenCommand(
            Email: email,
            Password: password);

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"GenerateValidToken failed with error: {result.TopError.Code} - {result.TopError.Description}");
        }

        return result.Value;
    }

    public static async Task<TokenDto> GenerateValidManagerToken(IMediator mediator, CancellationToken ct = default)
    {
        var user = TestUsers.Manager;

        return await GenerateValidToken(mediator, user.Email!, user.Email!, ct);
    }
    public static async Task<TokenDto> GenerateValidLaborToken(IMediator mediator, CancellationToken ct = default)
    {
        var user = TestUsers.Labor01;

        return await GenerateValidToken(mediator, user.Email!, user.Email!, ct);
    }
}
