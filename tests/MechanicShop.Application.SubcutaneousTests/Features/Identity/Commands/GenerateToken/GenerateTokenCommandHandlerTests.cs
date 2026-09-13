using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Utilities;
using MechanicShop.Application.Features.Identity.Commands.GenerateToken;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Infrastructure.Identity;
using MechanicShop.Tests.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Commands.GenerateToken;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GenerateTokenCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public GenerateTokenCommandHandlerTests(WebAppFactory factory)
    {
        _factory = factory;

        (_mediator, _context, _scope) = factory.CreateMediatorAndAppDbContext();
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    [Theory]
    [MemberData(nameof(GetInvalidCredentials))]
    public async Task Handle_ShouldFail_WhenCredentialsInvalid((string Email, string Password) credentials)
    {
        var ct = CancellationToken.None;

        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

        var command = new GenerateTokenCommand(
     Email: credentials.Email!,
     Password: credentials.Password!);


        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Unauthorized, result.TopError.Type);
    }


    [Theory]
    [MemberData(nameof(GetValidUsers))]
    public async Task Handle_ShouldSuccess_WhenValidData(AppUser user)
    {
        var ct = CancellationToken.None;

        _factory.FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

        var command = new GenerateTokenCommand(
            Email: user.Email!,
            Password: user.Email!);


        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        var tokenDto = result.Value;
        Assert.NotNull(tokenDto);

        Assert.True(tokenDto.ExpiresOnUtc > _factory.FakeTimeProvider.GetUtcNow());
        var tokenHashed = HashHelper.ComputeSha256(tokenDto.RefreshToken);
        var exists = await _context.RefreshTokens.AnyAsync(rt => rt.TokenHash == tokenHashed && !rt.IsRevoked, ct);
        Assert.True(exists);
    }

    public static TheoryData<(string, string)> GetInvalidCredentials => new TheoryData<(string, string)>()
    {
        (TestUsers.Manager.Email! +"dfd"!,TestUsers.Manager.Email! ),
        (TestUsers.Manager.Email!,TestUsers.Manager.Email + "dfd"),
        (TestUsers.Labor03.Email!,TestUsers.Labor03.Email + "dfd"),
        (TestUsers.Labor03.Email! + "dfd",TestUsers.Labor03.Email! ),
        ("unknownemail@gmail.com" ,"password1234" ),
    };

    public static TheoryData<AppUser> GetValidUsers => new TheoryData<AppUser>()
    {
        TestUsers.Manager,
        TestUsers.Labor01
    };
}
