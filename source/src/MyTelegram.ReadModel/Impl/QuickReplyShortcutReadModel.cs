namespace MyTelegram.ReadModel.Impl;

public class QuickReplyShortcutReadModel : ReadModelBase, IQuickReplyReadModel,
    IAmReadModelFor<QuickReplyShortcutAggregate, QuickReplyShortcutId, QuickReplyShortcutCreatedEvent>,
    IAmReadModelFor<QuickReplyShortcutAggregate, QuickReplyShortcutId, QuickReplyShortcutEditedEvent>,
    IAmReadModelFor<QuickReplyShortcutAggregate, QuickReplyShortcutId, QuickReplyShortcutDeletedEvent>
{
    public long UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public int ShortcutId { get; private set; }
    public List<int> MessageIds { get; private set; } = [];
    public virtual string Id { get; private set; } = null!;
    public virtual long? Version { get; set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<QuickReplyShortcutAggregate, QuickReplyShortcutId, QuickReplyShortcutCreatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Id = domainEvent.AggregateIdentity.Value;
        var e = domainEvent.AggregateEvent;
        UserId = e.UserId;
        ShortcutId = e.ShortcutId;
        Title = e.Name;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<QuickReplyShortcutAggregate, QuickReplyShortcutId, QuickReplyShortcutEditedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Title = domainEvent.AggregateEvent.Name;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<QuickReplyShortcutAggregate, QuickReplyShortcutId, QuickReplyShortcutDeletedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        context.MarkForDeletion();
        return Task.CompletedTask;
    }
}
