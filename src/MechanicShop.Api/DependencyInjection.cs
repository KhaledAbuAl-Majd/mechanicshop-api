using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using MechanicShop.Api.Infrastructure;
using MechanicShop.Api.OpenApi.Transformers;
using MechanicShop.Api.Services;
using MechanicShop.Api.Settings;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {

            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

            services.AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value);

            services.AddCustomProblemDetails()
                    .AddCustomApiVersioning()
                    .AddApiDocumentation()
                    .AddExceptionHandling()
                    .AddControllerWithJsonConfiguration()
                    .AddConfiguredCors(configuration)
                    .AddCurrentUserService()
                    .AddAppOutputCaching()
                    .AddAppRateLimiting(configuration)
                    .AddAppOpenTelememrty();

            return services;
        }


        private static IServiceCollection AddAppOutputCaching(this IServiceCollection services)
        {
            services.AddOutputCache(options =>
            {
                options.SizeLimit = 100 * 1024 * 1024;//100 mb

                options.AddBasePolicy(policy =>
                {
                    policy.Expire(TimeSpan.FromSeconds(60));
                });

                options.UseCaseSensitivePaths = false;
            });

            return services;
        }

        private static IServiceCollection AddAppOpenTelememrty(this IServiceCollection services)
        {
            services.AddOpenTelemetry()
              .ConfigureResource(res => res.AddService("mechanic-shop-service"))
              .WithTracing(tracing =>
              {
                  tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                  tracing.AddOtlpExporter();
              })
              .WithMetrics(metices =>
              {
                  metices.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                  metices.AddOtlpExporter()
                    .AddPrometheusExporter();
              });

            return services;
        }

        private static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            var rateLimitingSettings = configuration.GetSection(RateLimiterSettings.SectionName).Get<RateLimiterSettings>();

            ArgumentNullException.ThrowIfNull(rateLimitingSettings);

            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var path = httpContext.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

                    // تجاوز الـ Rate Limit للمسارات الداخلية والمراقبة
                    if (path.StartsWith("/metrics") || path.StartsWith("/health") || path.StartsWith("/scalar") || path.StartsWith("/openapi"))
                    {
                        return RateLimitPartition.GetNoLimiter("no_limit");
                    }

                    var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    var partitionKey = !string.IsNullOrWhiteSpace(userId) ? $"user_{userId}" : $"ip_{ip}";

                    return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitingSettings.Global.PermitLimit,
                        QueueLimit = rateLimitingSettings.Global.QueueLimit,
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        Window = TimeSpan.FromMinutes(rateLimitingSettings.Global.WindowInMinutes),
                        SegmentsPerWindow = rateLimitingSettings.Global.SegmentsPerWindow
                    });
                });

                options.AddPolicy(AuthRateLimiterOptions.PolicyName,
                    httpContext => RateLimitPartition.GetSlidingWindowLimiter(partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()?? "unknown",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitingSettings.Auth.PermitLimit,
                        QueueLimit = rateLimitingSettings.Auth.QueueLimit,
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        Window = TimeSpan.FromMinutes(rateLimitingSettings.Auth.WindowInMinutes),
                        SegmentsPerWindow = rateLimitingSettings.Auth.SegmentsPerWindow
                    }));

                options.AddConcurrencyLimiter(ConcurrencyRateLimiterOptions.PolicyName, limiterOptions =>
                {
                    limiterOptions.PermitLimit = rateLimitingSettings.HeavyExport.PermitLimit;
                    limiterOptions.QueueLimit = rateLimitingSettings.HeavyExport.QueueLimit;
                });

                options.OnRejected = async (context, ct) =>
                {

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    int retryAfterSeconds = 0;


                   //it's a microsfot problem - lease is emtpty
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter,out var retryAfter))
                    {
                        retryAfterSeconds = (int)retryAfter.TotalSeconds;
                        context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();
                    }

                    var problemDetailsService = context.HttpContext.RequestServices.GetService<IProblemDetailsService>();

                    if(problemDetailsService is not null)
                    {
                        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                        {
                            HttpContext = context.HttpContext,
                            ProblemDetails = new ProblemDetails
                            {
                                Status = StatusCodes.Status429TooManyRequests,
                                Title = "Too Many Requests",
                                Detail = retryAfterSeconds > 0
                                ? $"Rate limit exceeded. Please try again after {retryAfterSeconds} seconds."
                                : "Rate limit exceeded. Please try again later.",
                            }
                        });
                    }
                };
            });

            return services;
        }

        private static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
        {
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                    context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
                };
            });

            return services;
        }

        private static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }

        private static IServiceCollection AddApiDocumentation(this IServiceCollection services)
        {
            string[] versions = ["v1"];

            foreach (var version in versions)
            {
                services.AddOpenApi(version, options =>
                {
                    options.AddDocumentTransformer<VersionInfoTransformer>();

                    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

                    options.AddOperationTransformer<BearerSecurityOperationTransformer>();

                    options.AddOperationTransformer<IdempotencyHeaderOperationTransformer>();
                });
            }


            return services;
        }

        private static IServiceCollection AddExceptionHandling(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }

        private static IServiceCollection AddControllerWithJsonConfiguration(this IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });


            return services;
        }

        private static IServiceCollection AddCurrentUserService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.TryAddScoped<IUser, CurrentUser>();

            return services;
        }

        private static IServiceCollection AddConfiguredCors(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();

            ArgumentNullException.ThrowIfNull(appSettings);

            services.AddCors(options =>
            {
                options.AddPolicy(appSettings.CorsPolicyName, policy =>
                {
                    //if empty will prevent any origins
                    if (appSettings.AllowedOrigins.Length != 0)
                    {
                        policy.WithOrigins(appSettings.AllowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    }
                });
            });

            return services;
        }

        public static IApplicationBuilder UseCoreMiddlewares(this IApplicationBuilder app, IConfiguration configuration)
        {
            var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();

            ArgumentNullException.ThrowIfNull(appSettings);

            app.UseExceptionHandler();

            app.UseStatusCodePages();

            app.UseHttpsRedirection();

            app.UseOpenTelemetryPrometheusScrapingEndpoint();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseCors(appSettings.CorsPolicyName);

            app.UseAuthentication();

            app.UseRateLimiter();

            app.UseAuthorization();

            app.UseOutputCache();

            return app;
        }
    }
}
