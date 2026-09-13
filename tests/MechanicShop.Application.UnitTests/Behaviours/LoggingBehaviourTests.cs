using MechanicShop.Application.Common.Behaviours;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MechanicShop.Application.UnitTests.Behaviours;

public class LoggingBehaviourTests
{
    private readonly ILogger<DummyRequest> _logger = Substitute.For<ILogger<DummyRequest>>();
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();

    private readonly LoggingBehaviour<DummyRequest> _sut;

    private const string logMessageKeyWord = "Request";

    public LoggingBehaviourTests()
    {
        _sut = new LoggingBehaviour<DummyRequest>(_logger, _user, _identityService);
    }

    [Fact]
    public async Task Proces_ShouldLogRequestWithUserName_WithUserId()
    {
        // arrange
        var request = new DummyRequest();
        string userId = "abc123";
        string userName = "khaled";

        _user.Id.Returns(userId);
        _identityService.GetUserNameAsync(userId).Returns(userName);

        //act
        await _sut.Process(request, CancellationToken.None);

        //assert
        await _identityService.Received(1).GetUserNameAsync(userId);

        _logger.Received(1)
            .Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(logMessageKeyWord)),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

    }

    [Fact]
    public async Task Proces_ShouldLogRequestWithEmptyUserName_WithoutUserId()
    {
        //arrange
        var reqeust = new DummyRequest();

        _user.Id.ReturnsNull();

        //act
        await _sut.Process(reqeust, CancellationToken.None);

        //assert
        await _identityService.DidNotReceive().GetUserNameAsync(Arg.Any<string>());


        _logger.Received(1)
           .Log(
           LogLevel.Information,
           Arg.Any<EventId>(),
           Arg.Is<object>(o => o.ToString()!.Contains(logMessageKeyWord)),
           Arg.Any<Exception>(),
           Arg.Any<Func<object, Exception?, string>>());
    }

    public class DummyRequest;
}
