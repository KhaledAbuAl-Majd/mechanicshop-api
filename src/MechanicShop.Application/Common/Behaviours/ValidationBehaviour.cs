using FluentValidation;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Common.Results.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviours
{
    public class ValidationBehaviour<TRequest, TResponse>(ILogger<ValidationBehaviour<TRequest, TResponse>> Logger,
        IEnumerable<IValidator<TRequest>>? Validators = null) :
        IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult//TRespnonse implement IResult to can return list<errors> (dynamic) conversion
    {
        private readonly IEnumerable<IValidator<TRequest>>? _validators = Validators;
        private readonly ILogger<ValidationBehaviour<TRequest, TResponse>> _logger = Logger;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            if (_validators is null || !_validators.Any())
            {
                return await next();
            }

            _logger.LogInformation("Validating {RequestName}", typeof(TRequest).Name);

            var validateResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(request, ct)));
            var failures = validateResults.SelectMany(v => v.Errors).Where(e => e is not null).ToList();

            if (failures.Count == 0)
            {
                return await next();
            }

            _logger.LogWarning("Validation failed for {RequestName} with {ErrorCount} errors",
            typeof(TRequest).Name, failures.Count);

            var errors = failures.ConvertAll(error => Error.Validation(code: !string.IsNullOrWhiteSpace(error.ErrorCode) ? error.ErrorCode : error.PropertyName,
                description: error.ErrorMessage));

            return (dynamic)errors;
        }
    }
}
