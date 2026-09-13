using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MechanicShop.Api.OpenApi.Transformers
{
    public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            string schemeId = JwtBearerDefaults.AuthenticationScheme;

            document.Components ??= new();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

            var scheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = schemeId.ToLowerInvariant(),
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "JWT Authorization header using Bearer scheme.",

            };

            document.Components.SecuritySchemes[schemeId] = scheme;

            return Task.CompletedTask;

        }
    }
}
