using MechanicShop.Application.Common.Behaviours;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MechanicShop.Application.UnitTests.Behaviours
{
    public class CachingBehviourTests
    {
        private readonly HybridCache _cache = Substitute.For<HybridCache>();
        private readonly ILogger<CachingBehviour<CachedQuery, Result<string>>> _logger = Substitute.For<ILogger<CachingBehviour<CachedQuery, Result<string>>>>();

        private readonly CachingBehviour<CachedQuery, Result<string>> _sut;

        public CachingBehviourTests()
        {
            _sut = new CachingBehviour<CachedQuery, Result<string>>(_cache, _logger);
        }

        [Fact]
        public async Task Handle_ShouldSkipCacheAndReturnResult_WhenNotCachedQuery()
        {
            string resultMessage = "OK";
            var request = new NonCachedQuery();
            var behaviour = new CachingBehviour<NonCachedQuery, string>(_cache, Substitute.For<ILogger<CachingBehviour<NonCachedQuery, string>>>());

            var result = await behaviour.Handle(request, () => Task.FromResult(resultMessage), CancellationToken.None);

            Assert.Equal(resultMessage, result);

            await _cache.DidNotReceive().SetAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<string[]>(),
                Arg.Any<CancellationToken>());
        }


        [Fact]
        public async Task Handle_ShouldCacheResult_WhenResultIsSuccess()
        {
            string responseValue = "test-value";
            var request = new CachedQuery();
            var response = (Result<string>)responseValue;

            string? actualKey = null;
            object? actualValue = null;
            HybridCacheEntryOptions? acutalOptions = null;
            string[]? actualTags = null;
            CancellationToken actualToken = default;

            _cache.SetAsync(
                Arg.Do<string>(k => actualKey = k),
                Arg.Do<Result<string>>(v => actualValue = v),
                Arg.Do<HybridCacheEntryOptions>(o => acutalOptions = o),
                Arg.Do<string[]>(t => actualTags = t),
                Arg.Do<CancellationToken>(ct => actualToken = ct))
                .Returns(ValueTask.CompletedTask);

            var result = await _sut.Handle(request, () => Task.FromResult(response), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(request.CacheKey, actualKey);

            var typed = Assert.IsType<Result<string>>(actualValue);
            Assert.True(typed.IsSuccess);
            Assert.Equal(responseValue, typed.Value);
            Assert.Equal(request.Expiration, acutalOptions!.Expiration);
            Assert.Equal(request.Tags, actualTags);
        }


        [Fact]
        public async Task Handle_ShouldNotCacheResult_WhenResultIsError()
        {
            var error = Error.Validation();
            var request = new CachedQuery();
            var response = (Result<string>)error;

            var result = await _sut.Handle(request, () => Task.FromResult(response), CancellationToken.None);

            Assert.False(result.IsSuccess);

            await _cache.DidNotReceive()
                .SetAsync(
                Arg.Any<string>(),
                Arg.Any<Result<string>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<string[]>(),
                Arg.Any<CancellationToken>());
        }

        public class NonCachedQuery;

        public class CachedQuery : ICachedQuery
        {
            public string CacheKey => "test-key";

            public string[] Tags => ["unit-test"];

            public TimeSpan Expiration => TimeSpan.FromMinutes(5);
        }

        //public class DummyRequest;
    }
}
