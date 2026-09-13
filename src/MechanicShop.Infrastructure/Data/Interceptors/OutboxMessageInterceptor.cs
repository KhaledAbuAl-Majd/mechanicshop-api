using System.Text.Json;
using MechanicShop.Domain.Common;
using MechanicShop.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MechanicShop.Infrastructure.Data.Interceptors;

public class OutboxMessageInterceptor(TimeProvider provider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AddOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }


    private void AddOutboxMessages(DbContext? context)
    {
        if (context is null)
            return;

        var domainEntites = context.ChangeTracker.Entries()
            .Where(e => e.Entity is Entity baseEntity && baseEntity.DomainEvents.Count != 0)
            .Select(e => (Entity)e.Entity)
            .ToList();

        var domainEvents = domainEntites.SelectMany(e => e.DomainEvents).ToList();

        var outboxMessages = new List<OutboxMessage>(domainEvents.Count);

        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = OutboxMessage.Create(
                type: domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().Name,
                content: JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                provider: provider);

            outboxMessages.Add(outboxMessage);
        }

        foreach (var entity in domainEntites)
            entity.ClearDomainEvents();

        context.AddRange(outboxMessages);
    }
}
