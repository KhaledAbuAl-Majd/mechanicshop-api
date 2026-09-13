using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Api.Infrastructure
{
    public class GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            var (statusCode, title, detail) = exception switch
            {
                DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Concurrency Conflict",
                "The record was modified or deleted by another operation."),

                DbUpdateException => (
                    StatusCodes.Status409Conflict,
                    "Database Constraint Violation",
                    "A data integrity rule was violated."),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred on the server.")
            };

            httpContext.Response.StatusCode = statusCode;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Type = exception.GetType().Name,
                    Title = title,
                    Detail = detail
                }
            });
        }
    }
}
