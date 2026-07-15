namespace MyTelegram.ReadModel.Impl;

public class RecentStickerReadModel : ReadModelBase, IRecentStickerReadModel,
    IAmReadModelFor<RecentStickerAggregate, RecentStickerId, SaveRecentStickerEvent>,
    IAmReadModelFor<RecentStickerAggregate, RecentStickerId, UnsaveRecentStickerEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }

    public long UserId { get; private set; }
    public long DocumentId { get; private set; }
    public bool Attached { get; private set; }
    public int Date { get; private set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<RecentStickerAggregate, RecentStickerId, SaveRecentStickerEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Id = domainEvent.AggregateIdentity.Value;
        UserId = e.UserId;
        DocumentId = e.DocumentId;
        Attached = e.Attached;
        Date = e.Date;
        IsDeleted = false;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<RecentStickerAggregate, RecentStickerId, UnsaveRecentStickerEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        IsDeleted = true;
        return Task.CompletedTask;
    }
}
