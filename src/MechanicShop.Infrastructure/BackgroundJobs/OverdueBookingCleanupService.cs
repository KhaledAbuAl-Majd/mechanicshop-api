using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Settings;
using MechanicShop.Domain.WorkOrders.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Infrastructure.BackgroundJobs
{
    public class OverdueBookingCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<OverdueBookingCleanupService> logger,
        TimeProvider datetime,
        AppSettings appsettings) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<OverdueBookingCleanupService> _logger = logger;
        private readonly TimeProvider _datetime = datetime;
        private readonly AppSettings _appSettings = appsettings;
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            //IDisposable
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_appSettings.OverdueBookingCleanupFrequencyMinutes), _datetime);

            int failedCount = 0;

            while (await timer.WaitForNextTickAsync(ct))
            {
                _logger.LogInformation("Checking overdue work orders at {Now}", _datetime.GetUtcNow());


                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                    var cutoff = _datetime.GetUtcNow().AddMinutes(-1 * _appSettings.BookingCancellationThresholdMinutes);
                    var overdue = await db.WorkOrders
                        .Where(wo => wo.State == WorkOrderState.Scheduled && wo.StartAtUtc <= cutoff)
                        .ToListAsync(ct);



                    if (overdue.Count > 0)
                    {
                        int successCancelCount = 0;

                        foreach (var wo in overdue)
                        {
                            var cancelResult = wo.Cancel();

                            if (cancelResult.IsError)
                            {
                                _logger.LogWarning("Failed to cancel WorkOrder {Id}: {@Errors}", wo.Id, cancelResult.Errors);
                            }
                            else
                            {
                                successCancelCount++;
                            }
                        }

                        await db.SaveChangesAsync(ct);

                        _logger.LogInformation(
                        "Processed {Total} overdue work orders (Cancelled: {Success}, Failed: {Failed}). IDs: {Ids}",
                        overdue.Count,
                        successCancelCount,
                        overdue.Count - successCancelCount,
                        overdue.Select(w => w.Id));

                    }
                    else
                    {
                        _logger.LogInformation("No overdue work orders found.");
                    }

                    failedCount = 0;
                }
                catch (Exception ex) when (!ct.IsCancellationRequested)
                {
                    failedCount++;
                    _logger.LogError(ex, "Error cleaning up overdue work orders.");

                    if (failedCount == 5)
                    {
                        _logger.LogCritical(ex, "Error cleaning up overdue failed 5 attemps");
                        failedCount = 0;
                    }
                }
            }
        }
    }
}
