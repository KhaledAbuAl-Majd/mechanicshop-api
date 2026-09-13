using MechanicShop.Application.Common.Behaviours;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MechanicShop.Application.UnitTests.Behaviours;

public class PerformanceBehaviourTests
{
    private readonly ILogger<PerformanceBehaviour<TestRequest, string>> _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    private readonly PerformanceBehaviour<TestRequest, string> _sut;

    private const int LongRunningRequestThresholdInMilliseconds = 500;

    private string logMessageKeyWords = "Long Running Request";

    public PerformanceBehaviourTests()
    {
        _logger = Substitute.For<ILogger<PerformanceBehaviour<TestRequest, string>>>();
        _user = Substitute.For<IUser>();
        _identityService = Substitute.For<IIdentityService>();

        _sut = new PerformanceBehaviour<TestRequest, string>(_logger, _user, _identityService);
    }

    [Fact]
    public async Task Handle_ShouldNotLogWarning_WhenRequestTakesLessThanOrEqual500MS()
    {
        var request = new TestRequest();
        string expectedResponse = "test-value";

        var result = await _sut.Handle(request, () => Task.FromResult(expectedResponse), CancellationToken.None);

        Assert.Equal(expectedResponse, result);
        _logger.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_WhenRequestTakesMoreThan500MSAndValidData()
    {
        var request = new TestRequest();
        string expectedResponse = "test-value";
        var cancellationToken = CancellationToken.None;
        const string userId = "abc123";
        const string userName = "khaled";

        _user.Id.Returns(userId);
        _identityService.GetUserNameAsync(userId).Returns(userName);

        var result = await _sut.Handle(request, async () =>
        {
            await Task.Delay(LongRunningRequestThresholdInMilliseconds + 100, cancellationToken);
            return expectedResponse;
        },
        cancellationToken);

        Assert.Equal(expectedResponse, result);
        await _identityService.Received(1).GetUserNameAsync(userId);

        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o =>
                o.ToString()!.Contains(logMessageKeyWords) &&
                o.ToString()!.Contains(nameof(TestRequest)) &&
                o.ToString()!.Contains(userId) &&
                o.ToString()!.Contains(userName)),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Handle_ShouldLogWarningWithEmptyUserData_WhenUserIdIsInvalid(string? userId)
    {
        var request = new TestRequest();
        string expectedResponse = "test-value";
        var cancellationToken = CancellationToken.None;

        _user.Id.Returns(userId);

        var result = await _sut.Handle(request, async () =>
        {
            await Task.Delay(LongRunningRequestThresholdInMilliseconds + 100, cancellationToken);
            return expectedResponse;
        },
        cancellationToken);


        Assert.Equal(expectedResponse, result);
        await _identityService.DidNotReceive().GetUserNameAsync(Arg.Any<string>());

        _logger.Received(1).Log(
           LogLevel.Warning,
           Arg.Any<EventId>(),
           Arg.Is<object>(o =>
               o.ToString()!.Contains(logMessageKeyWords) &&
               o.ToString()!.Contains(nameof(TestRequest))),
           Arg.Any<Exception?>(),
           Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ShouldLogWarningWithEmptyUserName_WhenIdentityServiceReturnsNull()
    {
        var request = new TestRequest();
        string expectedResponse = "test-value";
        var cancellationToken = CancellationToken.None;

        const string userId = "abc123";
        const string? userName = null;

        _user.Id.Returns(userId);
        _identityService.GetUserNameAsync(userId).Returns(userName);

        var result = await _sut.Handle(request, async () =>
        {
            await Task.Delay(LongRunningRequestThresholdInMilliseconds + 100, cancellationToken);
            return expectedResponse;
        },
        cancellationToken);


        Assert.Equal(expectedResponse, result);
        await _identityService.Received(1).GetUserNameAsync(userId);

        _logger.Received(1).Log(
           LogLevel.Warning,
           Arg.Any<EventId>(),
           Arg.Is<object>(o =>
               o.ToString()!.Contains(logMessageKeyWords) &&
               o.ToString()!.Contains(nameof(TestRequest)) &&
               o.ToString()!.Contains(userId)),
           Arg.Any<Exception?>(),
           Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ShouldNotCatchException_WhenNextThrowsException()
    {
        var request = new TestRequest();
        var ct = CancellationToken.None;
        var expectedException = new ArgumentException("argument is invalid");

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.Handle(request, () => throw expectedException, ct));

        Assert.Equal(expectedException, exception);

        await _identityService.DidNotReceive().GetUserNameAsync(Arg.Any<string>());
        _logger.DidNotReceive().Log(
           LogLevel.Warning,
           Arg.Any<EventId>(),
           Arg.Any<object>(),
           Arg.Any<Exception?>(),
           Arg.Any<Func<object, Exception?, string>>());
    }

    public class TestRequest;
}
