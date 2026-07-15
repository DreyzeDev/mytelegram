namespace MyTelegram.ReadModel.Impl;

public class StoryReadModel : ReadModelBase, IStoryReadModel,
    IAmReadModelFor<StoryAggregate, StoryId, StoryCreatedEvent>,
    IAmReadModelFor<StoryAggregate, StoryId, StoryDeletedEvent>,
    IAmReadModelFor<StoryAggregate, StoryId, StoryEditedEvent>,
    IAmReadModelFor<StoryAggregate, StoryId, StoryPinnedToggledEvent>,
    IAmReadModelFor<StoryAggregate, StoryId, StoryViewIncrementedEvent>
{
    public long OwnerPeerId { get; private set; }
    public int StoryId { get; private set; }
    public IMessageMedia Media { get; private set; } = null!;
    public long RandomId { get; private set; }
    public List<PrivacyValueData> PrivacyRules { get; private set; } = [];
    public int Date { get; private set; }
    public int ExpireDate { get; private set; }
    public Peer? FromPeer { get; private set; }
    public string? Caption { get; private set; }
    public List<IMediaArea>? MediaAreas { get; private set; }
    public bool Pinned { get; private set; }
    public bool NoForwards { get; private set; }
    public List<IMessageEntity>? Entities { get; private set; }
    public int? Period { get; private set; }
    public Peer? FwdFromId { get; private set; }
    public int? FwdFromStory { get; private set; }
    public bool Archived { get; private set; }
    public int ViewsCount { get; private set; }
    public List<long>? RecentViewers { get; private set; }
    public virtual string Id { get; private set; } = null!;
    public virtual long? Version { get; set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<StoryAggregate, StoryId, StoryCreatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Id = domainEvent.AggregateIdentity.Value;
        var item = domainEvent.AggregateEvent.StoryItem;
        OwnerPeerId = item.Peer.PeerId;
        StoryId = item.Id;
        Media = item.Media;
        RandomId = item.RandomId;
        PrivacyRules = item.PrivacyRules;
        Date = item.Date;
        ExpireDate = item.ExpireDate;
        FromPeer = item.FromPeer;
        Caption = item.Caption;
        MediaAreas = item.MediaAreas;
        Pinned = item.Pinned;
        NoForwards = item.NoForwards;
        Entities = item.Entities;
        Period = item.Period;
        FwdFromId = item.FwdFromId;
        FwdFromStory = item.FwdFromStory;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<StoryAggregate, StoryId, StoryDeletedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        context.MarkForDeletion();
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<StoryAggregate, StoryId, StoryEditedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var item = domainEvent.AggregateEvent.StoryItem;
        Media = item.Media;
        Caption = item.Caption;
        MediaAreas = item.MediaAreas;
        PrivacyRules = item.PrivacyRules;
        Entities = item.Entities;
        Pinned = item.Pinned;
        NoForwards = item.NoForwards;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<StoryAggregate, StoryId, StoryPinnedToggledEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Pinned = domainEvent.AggregateEvent.Pinned;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<StoryAggregate, StoryId, StoryViewIncrementedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        ViewsCount++;
        RecentViewers ??= [];
        var viewerUserId = domainEvent.AggregateEvent.ViewerUserId;
        if (!RecentViewers.Contains(viewerUserId))
            RecentViewers.Add(viewerUserId);
        if (RecentViewers.Count > 20)
            RecentViewers.RemoveAt(0);
        return Task.CompletedTask;
    }
}
