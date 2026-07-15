namespace MyTelegram.ReadModel.Impl;

public class FaveStickerReadModel : ReadModelBase, IFaveStickerReadModel,
    IAmReadModelFor<FaveStickerAggregate, FaveStickerId, FaveStickerEvent>,
    IAmReadModelFor<FaveStickerAggregate, FaveStickerId, UnfaveStickerEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }

    public long UserId { get; private set; }
    public long DocumentId { get; private set; }
    public int Date { get; private set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<FaveStickerAggregate, FaveStickerId, FaveStickerEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Id = domainEvent.AggregateIdentity.Value;
        UserId = e.UserId;
        DocumentId = e.DocumentId;
        Date = e.Date;
        IsDeleted = false;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<FaveStickerAggregate, FaveStickerId, UnfaveStickerEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        IsDeleted = true;
        return Task.CompletedTask;
    }
}
