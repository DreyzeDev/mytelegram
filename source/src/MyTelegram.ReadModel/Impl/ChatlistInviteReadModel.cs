namespace MyTelegram.ReadModel.Impl;

public class ChatlistInviteReadModel : ReadModelBase, IChatlistInviteReadModel,
    IAmReadModelFor<ChatlistInviteAggregate, ChatlistInviteId, ChatlistInviteCreatedEvent>,
    IAmReadModelFor<ChatlistInviteAggregate, ChatlistInviteId, ChatlistInviteEditedEvent>,
    IAmReadModelFor<ChatlistInviteAggregate, ChatlistInviteId, ChatlistInviteDeletedEvent>
{
    public long UserId { get; private set; }
    public int FilterId { get; private set; }
    public string Slug { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string PeersJson { get; private set; } = "[]";
    public string? Emoticon { get; private set; }
    public virtual string Id { get; private set; } = null!;
    public virtual long? Version { get; set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<ChatlistInviteAggregate, ChatlistInviteId, ChatlistInviteCreatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Id = domainEvent.AggregateIdentity.Value;
        var e = domainEvent.AggregateEvent;
        UserId = e.UserId;
        FilterId = e.FilterId;
        Slug = e.Slug;
        Title = e.Title;
        PeersJson = e.PeersJson;
        Emoticon = e.Emoticon;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<ChatlistInviteAggregate, ChatlistInviteId, ChatlistInviteEditedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Title = e.Title;
        if (e.PeersJson != null)
            PeersJson = e.PeersJson;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<ChatlistInviteAggregate, ChatlistInviteId, ChatlistInviteDeletedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        context.MarkForDeletion();
        return Task.CompletedTask;
    }
}
