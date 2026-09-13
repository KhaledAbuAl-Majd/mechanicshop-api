using System.Text;
using MechanicShop.Application.Common;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Settings;
using MechanicShop.Application.Features.Billing.Interfaces;
using MechanicShop.Application.Features.Identity.Interfaces;
using MechanicShop.Domain.Identity.Enums;
using MechanicShop.Infrastructure.BackgroundJobs;
using MechanicShop.Infrastructure.Data;
using MechanicShop.Infrastructure.Data.Interceptors;
using MechanicShop.Infrastructure.Identity;
using MechanicShop.Infrastructure.Identity.Polices;
using MechanicShop.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        ArgumentNullException.ThrowIfNull(connectionString);

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

        ArgumentNullException.ThrowIfNull(jwtSettings);

        services.TryAddSingleton(jwtSettings);


        QuestPDF.Settings.License = LicenseType.Community;

        services.AddBackgroundServices();
        services.AddData(connectionString);
        services.AddIdentity(jwtSettings);
        services.AddServices();
        services.AddCaching(configuration);

        return services;
    }

    private static IServiceCollection AddData(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, OutboxMessageInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<ApplicationDbContextInitialiser>();

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequiredUniqueChars = 1;
            options.SignIn.RequireConfirmedAccount = false;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        return services;
    }

    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<OverdueBookingCleanupService>();
        services.AddHostedService<OutboxProcessorBackgroundService>();

        return services;
    }

    private static IServiceCollection AddIdentity(this IServiceCollection services, JwtSettings jwtSettings)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
            };
        });

        services.AddScoped<IAuthorizationHandler, LaborAssignedHandler>();
        services.AddScoped<IAuthorizationHandler, UserOwnerOrManagerHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.ManagerOnly, policy => policy.RequireRole(nameof(Role.Manager)))

            .AddPolicy(AuthorizationPolicies.SelfScopedWorkOrderAccess, policy =>
            policy.Requirements.Add(new LaborAssignedRequirement()))

            .AddPolicy(AuthorizationPolicies.UserOwnerOrManager, policy =>
            {
                policy.Requirements.Add(new UserOwnerOrManagerRequirement());
            });


        services.TryAddScoped<IIdentityService, IdentityService>();
        services.TryAddScoped<ITokenProvider, TokenProvider>();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {

        services.TryAddSingleton<IInvoicePdfGenerator, InvoicePdfGenerator>();
        services.TryAddSingleton<INotificationService, NotificationService>();
        services.TryAddSingleton<IWorkOrderNotifier, WorkOrderNotifier>();

        return services;
    }

    private static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "MechanicShop:01:";
            });
        }

        services.AddHybridCache(options =>
        {
            //default options (if you don't override it)
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),//distrubuted cache - L2
                LocalCacheExpiration = TimeSpan.FromSeconds(40)//Memory cache - L1
            };
        });

        return services;
    }
}
