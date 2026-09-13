using MechanicShop.Application.Common.Behaviours;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MechanicShop.Application.UnitTests.Behaviours;

public class CacheInvalidationBehaviourTests
{
    private readonly ILogger<CacheInvalidationBehaviour<InvalidateCacheCommand, Result<string>>> _logger = Substitute.For<ILogger<CacheInvalidationBehaviour<InvalidateCacheCommand, Result<string>>>>();
    private readonly HybridCache _cache = Substitute.For<HybridCache>();

    private CacheInvalidationBehaviour<InvalidateCacheCommand, Result<string>> _sut;

    public CacheInvalidationBehaviourTests()
    {
        _sut = new CacheInvalidationBehaviour<InvalidateCacheCommand, Result<string>>(_logger, _cache);
    }

    [Fact]
    public async Task Handle_ShouldSkipInvalidatingCache_WhenTagsIsEmpty()
    {
        var request = new InvalidateCacheCommand();
        request.Tags = [];
        var response = (Result<string>)"test-value";

        var result = await _sut.Handle(request, () => Task.FromResult(response), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cache.DidNotReceive()
            .RemoveByTagAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        Assert.Equal(response.Value, result.Value);
    }

    [Fact]
    public async Task Handle_ShouldSkipInvalidatingCache_WhenResultIsError()
    {
        var error = Error.Conflict();
        var request = new InvalidateCacheCommand();
        var response = (Result<string>)error;

        var result = await _sut.Handle(request, () => Task.FromResult(response), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _cache.DidNotReceive()
                    .RemoveByTagAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [MemberData(nameof(GetTags))]
    public async Task Handle_ShouldInvalidateCache_WhenResultIsSuccessAndTagsIsNotEmpty(string[] tags)
    {
        var request = new InvalidateCacheCommand();
        request.Tags = tags;
        var response = (Result<string>)"test-value";

        //var tagsCount = request.Tags.Count();

        var result = await _sut.Handle(request, () => Task.FromResult(response), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(response.Value, result.Value);
        foreach (var tag in request.Tags)
        {
            await _cache.Received(1).RemoveByTagAsync(tag);
        }
    }

    public static TheoryData<string[]> GetTags() => new TheoryData<string[]>()
    {
       new string[] {"test"},
       new string[] {"test1","test2"},
       new string[] {"test3","test4","test5"},
    };
}


public class InvalidateCacheCommand : IInvalidateCacheCommand
{
    public string[] Tags { get; set; } = ["test-tag"];
}

