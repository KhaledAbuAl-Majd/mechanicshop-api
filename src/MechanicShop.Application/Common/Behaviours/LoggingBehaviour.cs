using MechanicShop.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviours
{
    public class LoggingBehaviour<TRequest>(ILogger<TRequest> Logger,
        IUser User,
        IIdentityService IdentityService) : IRequestPreProcessor<TRequest>
        where TRequest : notnull
    {
        private readonly ILogger<TRequest> _logger = Logger;
        private readonly IUser _user = User;
        private readonly IIdentityService _identityService = IdentityService;

        public async Task Process(TRequest request, CancellationToken cancellationToken)
        {
            string requestName = typeof(TRequest).Name;
            var userId = _user.Id ?? string.Empty;
            string? userName = string.Empty;

            if (!string.IsNullOrEmpty(userId))
            {
                userName = await _identityService.GetUserNameAsync(userId);
            }

            _logger.LogInformation("Request: {Name} {UserId} {UserName} {@Request}", requestName, userId, userName, request);
        }
    }
}
