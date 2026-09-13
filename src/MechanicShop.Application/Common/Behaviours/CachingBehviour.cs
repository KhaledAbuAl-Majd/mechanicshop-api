using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results.Abstractions;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviours
{
    public class CachingBehviour<TRequest, TResponse>(HybridCache Cache, ILogger<CachingBehviour<TRequest, TResponse>> Logger) :
        IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly HybridCache _cache = Cache;
        private readonly ILogger<CachingBehviour<TRequest, TResponse>> _logger = Logger;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            if (request is not ICachedQuery cachedRequest)
            {
                return await next();
            }

            _logger.LogInformation("Checking cache for {RequestName}", typeof(TRequest).Name);

            var result = await _cache.GetOrCreateAsync(key: cachedRequest.CacheKey,
                factory: _ => new ValueTask<TResponse>((TResponse)(object)null!),
                options: new HybridCacheEntryOptions
                {
                    Flags = HybridCacheEntryFlags.DisableUnderlyingData //only fetch data - don't store it
                },
                  cancellationToken: ct);

            if (result is null)
            {
                //no cache found
                //no cache - wait handler result to cache it

                result = await next();

                if (result is IResult res && res.IsSuccess)
                {
                    _logger.LogInformation("Caching result for {RequstName}", typeof(TRequest).Name);
                    await _cache.SetAsync(key: cachedRequest.CacheKey,
                          value: result,
                          options: new HybridCacheEntryOptions
                          {
                              Expiration = cachedRequest.Expiration
                          },
                          tags: cachedRequest.Tags,
                          cancellationToken: ct);
                }
            }

            return result;
        }
    }
}
