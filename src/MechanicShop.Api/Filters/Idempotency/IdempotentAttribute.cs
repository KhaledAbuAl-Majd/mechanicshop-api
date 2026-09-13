using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Hybrid;

namespace MechanicShop.Api.Filters.Idempotency
{
    public class IdempotentAttribute : ActionFilterAttribute
    {

        private class CachedResponse
        {
            public int StatusCode { get; set; }
            public object? Value { get; set; }
        }

        private sealed class ActionFailedException : Exception
        {
            public IActionResult OrignalResult { get; }

            public ActionFailedException(IActionResult originalResult)
            {
                this.OrignalResult = originalResult;
            }
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var idempotencyKeyHeader = IdempotencyConstants.IdempotencyKeyHeader;

            if (!context.HttpContext.Request.Headers.TryGetValue(idempotencyKeyHeader, out var idempotencyKey) || string.IsNullOrWhiteSpace(idempotencyKey))
            {
                context.Result = Problem(StatusCodes.Status400BadRequest, "Bad Request", $"Missing or empty {idempotencyKeyHeader} header.");
                return;
            }

            var cache = context.HttpContext.RequestServices.GetRequiredService<HybridCache>();

            var normalizedPath = context.HttpContext.Request.Path.Value?.TrimEnd('/').ToLowerInvariant();
            var cacheKey = $"idempotency:{normalizedPath}:{idempotencyKey}";
            var cacnellationToken = context.HttpContext.RequestAborted;

            try
            {
                bool getFromCache = true;

                var result = await cache.GetOrCreateAsync(cacheKey, factory: async ct =>
                {
                    // no cache
                    getFromCache = false;

                    var executedContext = await next();


                    if (executedContext.Exception is not null && !executedContext.ExceptionHandled)
                    {
                        ExceptionDispatchInfo.Capture(executedContext.Exception).Throw();
                        //throw executedContext.Exception;
                    }

                    var statusCode = executedContext.Result switch
                    {
                        ObjectResult obj => obj.StatusCode ?? StatusCodes.Status200OK,
                        StatusCodeResult sc => sc.StatusCode,
                        _ => StatusCodes.Status200OK
                    };


                    if (statusCode < 200 || statusCode >= 300)//not success
                    {
                        throw new ActionFailedException(executedContext.Result!);
                    }

                    var responseToCache = new CachedResponse
                    {
                        StatusCode = statusCode,
                        Value = (executedContext.Result as ObjectResult)?.Value
                    };

                    return responseToCache;
                },
                cancellationToken: cacnellationToken,
                options: new HybridCacheEntryOptions
                {
                    LocalCacheExpiration = TimeSpan.FromMinutes(5),
                    Expiration = TimeSpan.FromMinutes(10),
                    //Flags = HybridCacheEntryFlags.DisableDistributedCache
                });

                if (getFromCache)
                {
                    context.Result = new ObjectResult(result.Value)
                    {
                        StatusCode = result.StatusCode
                    };
                }

            }
            catch (ActionFailedException)
            {
                //context.Result = ex.OrignalResult;
            }

        }

        private ObjectResult Problem(int statusCode, string title, string detail)
        {
            var problem = new ProblemDetails()
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            };

            return new ObjectResult(problem)
            {
                StatusCode = statusCode
            };
        }
    }
}
