using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviours
{
    public class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> Logger) : 
        IPipelineBehavior<TRequest, TResponse> 
        where TRequest : notnull
    {
        private readonly ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> _logger = Logger;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogError(ex, "Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);

                throw;
            }
        }
    }
}
