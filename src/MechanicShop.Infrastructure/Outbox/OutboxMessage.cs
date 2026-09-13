namespace MechanicShop.Infrastructure.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; }
        public string Type { get; } = default!;
        public string Content { get; } = default!;
        public DateTimeOffset OccurredOnUtc { get; }
        public DateTimeOffset? ProcessedOnUtc { get; private set; }
        public DateTimeOffset LastModifyOnUtc { get; private set; }
        public string? Error { get; private set; }
        public int RetryCount { get; private set; }

        private OutboxMessage() { }

        private OutboxMessage(Guid id, string type, string content, DateTimeOffset occurredOnUtc)
        {
            Id = id;
            Type = type;
            Content = content;
            OccurredOnUtc = occurredOnUtc;
            ProcessedOnUtc = null;
            Error = null;
            RetryCount = 0;
            LastModifyOnUtc = occurredOnUtc;
        }

        public static OutboxMessage Create(string type, string content, TimeProvider provider)
        {
            return new OutboxMessage(
                id: Guid.NewGuid(),
                type: type,
                content: content,
                occurredOnUtc: provider.GetUtcNow());
        }

        public void MarkAsProcessed(TimeProvider provider)
        {
            ProcessedOnUtc = provider.GetUtcNow();
            Error = null;
            LastModifyOnUtc = provider.GetUtcNow();
        }

        public void MarkAsFailed(string error, TimeProvider provider)
        {
            Error = error;
            RetryCount++;
            LastModifyOnUtc = provider.GetUtcNow();
        }
    }
}
