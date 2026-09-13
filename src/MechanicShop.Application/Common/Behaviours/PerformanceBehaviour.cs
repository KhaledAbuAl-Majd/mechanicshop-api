using System.Diagnostics;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviours
{
    /// <summary>
    /// Logging the long Running Requests
    /// </summary>
    public class PerformanceBehaviour<TRequest, TResponse>(
    ILogger<PerformanceBehaviour<TRequest, TResponse>> logger,
    IUser user,
    IIdentityService identityService)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger = logger;
        private readonly IUser _user = user;
        private readonly IIdentityService _identityService = identityService;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var timer = Stopwatch.StartNew();

            var response = await next();

            timer.Stop();

            var elapsedMilliseconds = timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                string requestName = typeof(TRequest).Name;
                var userId = _user.Id ?? string.Empty;
                string? userName = string.Empty;

                if (!string.IsNullOrEmpty(userId))
                {
                    userName = await _identityService.GetUserNameAsync(userId);
                }

                _logger.LogWarning("Long Running Request: {Name}, ({ElapsedMilliseconds} milliseconds) {UserId} {UserName} {@Request}",
                    requestName, elapsedMilliseconds, userId, userName, request);
            }

            return response;
        }
    }
}
