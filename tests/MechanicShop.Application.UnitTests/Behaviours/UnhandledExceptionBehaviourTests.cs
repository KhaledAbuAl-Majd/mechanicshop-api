using MechanicShop.Application.Common.Behaviours;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MechanicShop.Application.UnitTests.Behaviours;

public class UnhandledExceptionBehaviourTests
{
    private readonly ILogger<UnhandledExceptionBehaviour<DummyRequest, string>> _logger = Substitute.For<ILogger<UnhandledExceptionBehaviour<DummyRequest, string>>>();

    private const string logMessageKeyWord = "Unhandled Exception";

    private readonly UnhandledExceptionBehaviour<DummyRequest, string> _sut;
    public UnhandledExceptionBehaviourTests()
    {
        _sut = new UnhandledExceptionBehaviour<DummyRequest, string>(_logger);
    }

    [Fact]
    public async Task Handle_ShouldReturnResult_WhenNoException()
    {
        var responseValue = "OK";
        var request = new DummyRequest();

        var result = await _sut.Handle(request, () => Task.FromResult(responseValue), CancellationToken.None);

        Assert.Equal(responseValue, result);
    }

    [Fact]
    public async Task Handle_ShouldLogErrorAndRethrow_WhenExceptionThrown()
    {
        var request = new DummyRequest();
        var exception = new ArgumentException("argument is invalid");

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.Handle(request, () => throw exception, CancellationToken.None));

        Assert.Equal(exception, ex);

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(logMessageKeyWord)),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    public class DummyRequest;
}
