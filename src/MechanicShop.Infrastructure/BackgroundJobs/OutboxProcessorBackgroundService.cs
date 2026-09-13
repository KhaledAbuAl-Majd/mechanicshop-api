using System.Text.Json;
using MechanicShop.Domain.Common;
using MechanicShop.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Infrastructure.BackgroundJobs;

public class OutboxProcessorBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessorBackgroundService> logger,
    TimeProvider provider) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<OutboxProcessorBackgroundService> _logger = logger;
    private readonly TimeProvider _provider = provider;

    private const int ProcessFrequencyInSeconds = 10;
    private const int MaxRetries = 5;
    private const int BatchCount = 20;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(ProcessFrequencyInSeconds), _provider);
        int failedCount = 0;

        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                await ProcessMessagesAsync(ct);
                failedCount = 0;
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                failedCount++;
                _logger.LogError(ex, "Outbox processing loop failed unexpectedly.");

                if (failedCount == 5)
                {
                    failedCount = 0;
                    _logger.LogCritical(ex, "Outbox processing loop failed unexpectedly 5 attemps.");
                }
            }
        }
    }

    public async Task ProcessMessagesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var messages = await context.OutboxMessages
            .Where(o => o.ProcessedOnUtc == null && o.RetryCount < MaxRetries)
            .OrderBy(o => o.OccurredOnUtc)
            .Take(BatchCount)
            .ToListAsync(ct);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType is null)
                {
                    var error = $"Could not resolve event type: {message.Type}";
                    throw new InvalidOperationException(error);
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType) as DomainEvent;

                if (domainEvent is null)
                {
                    var error = "Deserialization content failed";
                    throw new InvalidOperationException(error);
                }

                using (var messageScope = _scopeFactory.CreateScope())
                {
                    var publisher = messageScope.ServiceProvider.GetRequiredService<IPublisher>();
                    await publisher.Publish(domainEvent, ct);
                }

                message.MarkAsProcessed(_provider);
            }
            catch (Exception ex)
            {
                var error = $"Outbox message with id '{message.Id}' processing failed: {ex.Message}";
                message.MarkAsFailed(error, _provider);

                _logger.LogError(ex, "Outbox message with id '{MessageId}' processing failed: {ErrorMessage}", message.Id, ex.Message);

                if (message.RetryCount >= MaxRetries)
                {
                    _logger.LogCritical("Outbox message with id '{MessageId}' processing failed permanently after {RetryCount} attempts",
                        message.Id, message.RetryCount);
                }
            }
        }

        await context.SaveChangesAsync(ct);
    }
}
