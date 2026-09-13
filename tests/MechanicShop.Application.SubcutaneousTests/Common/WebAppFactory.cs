using System.Data.Common;
using MechanicShop.Api;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Settings;
using MechanicShop.Infrastructure.BackgroundJobs;
using MechanicShop.Infrastructure.Data;
using MechanicShop.Tests.Common;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Respawn;
using Testcontainers.MsSql;

namespace MechanicShop.Application.SubcutaneousTests.Common;

public class WebAppFactory : WebApplicationFactory<IAssemblyMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    private Respawner _respawner = default!;

    private DbConnection _dbConnection = default!;

    public FakeTimeProvider FakeTimeProvider = new(DateTimeOffset.UtcNow);

    /// <summary>
    /// Create Mediator And AppDbContext in same scope
    /// </summary>
    /// <returns>tuble of IMediaor and IAppDbContext</returns>
    public (IMediator, IAppDbContext, IServiceScope) CreateMediatorAndAppDbContext()
    {
        var scope = Services.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        return (mediator, context, scope);
    }

    public IMediator CreateMediator()
    {
        var scope = Services.CreateScope();

        return scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        using var scope = Services.CreateScope();

        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        return await sender.Send(request, cancellationToken);
    }

    public IAppDbContext CreateAppDbContext()
    {
        var scope = Services.CreateScope();

        return scope.ServiceProvider.GetRequiredService<IAppDbContext>();
    }

    public AppSettings AppSettings => Services.GetRequiredService<AppSettings>();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            context.WorkOrders.RemoveRange(context.WorkOrders);
            await context.SaveChangesAsync();
        }

        _dbConnection = new SqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions()
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = ["dbo"],
            TablesToIgnore =
            [
                "__EFMigrationsHistory",
                "AspNetUsers",
                "AspNetRoles",
                "AspNetRoleClaims",
                "AspNetUserClaims",
                "AspNetUserLogins",
                "AspNetUserRoles",
                "AspNetUserTokens"]
        });

    }

    public new async Task DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
        await _dbContainer.DisposeAsync();

    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
        FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Redis", null);
        builder.UseSetting("JwtSettings:Secret", "YourSuperSecretKeyThatIsVeryLongAndSecure123456!");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<OverdueBookingCleanupService>();

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });

            services.RemoveAll<AppSettings>();
            services.RemoveAll<TimeProvider>();

            services.PostConfigure<AppSettings>(options =>
            {
                options.OpeningTime = AppSettingsTestData.DefaultOpeningTime;
                options.ClosingTime = AppSettingsTestData.DefaultClosingTime;
                options.MinimumAppointmentDurationInMinutes = AppSettingsTestData.MinimumAppointmentDurationInMinutes;
            });


            services.AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value);
            services.AddSingleton<TimeProvider>(FakeTimeProvider);
        });

    }
}
