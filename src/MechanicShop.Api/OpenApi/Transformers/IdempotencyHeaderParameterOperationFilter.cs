using MechanicShop.Api.Filters.Idempotency;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MechanicShop.Api.OpenApi.Transformers
{
    public class IdempotencyHeaderOperationTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var hasIdempotent = context.Description.ActionDescriptor.EndpointMetadata.OfType<IdempotentAttribute>().Any();

            if (!hasIdempotent)
                return Task.CompletedTask;

            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = IdempotencyConstants.IdempotencyKeyHeader,
                In = ParameterLocation.Header,
                Required = true,
                Description = "Unique UUID key to ensure idempotent request execution.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });

            return Task.CompletedTask;
        }
    }
}
