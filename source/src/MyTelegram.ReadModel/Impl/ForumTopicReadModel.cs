namespace MyTelegram.ReadModel.Impl;

public class ForumTopicReadModel : ReadModelBase, IForumTopicReadModel,
    IAmReadModelFor<ForumTopicAggregate, ForumTopicId, ForumTopicCreatedEvent>,
    IAmReadModelFor<ForumTopicAggregate, ForumTopicId, ForumTopicEditedEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }
    public long ChannelId { get; private set; }
    public int TopicId { get; private set; }
    public long CreatorUserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public int? IconColor { get; private set; }
    public long? IconEmojiId { get; private set; }
    public int Date { get; private set; }
    public bool Pinned { get; private set; }
    public bool Closed { get; private set; }
    public bool Hidden { get; private set; }
    public bool My { get; private set; }
    public int TopMessage { get; private set; }
    public int ReadInboxMaxId { get; private set; }
    public int ReadOutboxMaxId { get; private set; }
    public int UnreadCount { get; private set; }
    public int UnreadMentionsCount { get; private set; }
    public int UnreadReactionsCount { get; private set; }
    public Peer? SendAs { get; private set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<ForumTopicAggregate, ForumTopicId, ForumTopicCreatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Id = domainEvent.AggregateIdentity.Value;
        ChannelId = e.ChannelId;
        TopicId = e.TopicId;
        CreatorUserId = e.CreatorUserId;
        Title = e.Title;
        IconColor = e.IconColor;
        IconEmojiId = e.IconEmojiId;
        Date = e.Date;
        TopMessage = e.TopicId;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<ForumTopicAggregate, ForumTopicId, ForumTopicEditedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        if (e.Title != null) Title = e.Title;
        if (e.IconEmojiId.HasValue) IconEmojiId = e.IconEmojiId;
        if (e.Closed.HasValue) Closed = e.Closed.Value;
        if (e.Hidden.HasValue) Hidden = e.Hidden.Value;
        if (e.Pinned.HasValue) Pinned = e.Pinned.Value;
        return Task.CompletedTask;
    }
}
