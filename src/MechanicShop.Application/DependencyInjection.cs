using System.Reflection;
using FluentValidation;
using MechanicShop.Application.Common.Behaviours;
using MechanicShop.Application.Features.WorkOrders.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
            cfg.AddOpenBehavior(typeof(CachingBehviour<,>));
            cfg.AddOpenBehavior(typeof(CacheInvalidationBehaviour<,>));

            //cfg.AddRequestPostProcessor(typeof(CacheInvalidationBehaviour<,>));
        });

        services.TryAddScoped<IWorkOrderPolicy, WorkOrderPolicy>();

        return services;
    }
}