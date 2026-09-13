using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MechanicShop.Api.OpenApi.Transformers
{
    public sealed class BearerSecurityOperationTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var schemeId = JwtBearerDefaults.AuthenticationScheme;

            if (context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any())
            {
                operation.Security ??= [];

                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(schemeId, context.Document)] = []
                });
            }

            return Task.CompletedTask;
        }
    }
}
