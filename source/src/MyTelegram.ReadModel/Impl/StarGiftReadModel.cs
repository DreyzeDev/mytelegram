namespace MyTelegram.ReadModel.Impl;

public class StarGiftReadModel : ReadModelBase, IStarGiftReadModel,
    IAmReadModelFor<StarGiftAggregate, StarGiftId, StarGiftCreatedEvent>,
    IAmReadModelFor<StarGiftAggregate, StarGiftId, StarGiftAvailabilityUpdatedEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }
    public long GiftId { get; private set; }
    public long Stars { get; private set; }
    public long ConvertStars { get; private set; }
    public int AvailabilityTotal { get; private set; }
    public int AvailabilityRemains { get; private set; }
    public bool Limited { get; private set; }
    public bool SoldOut { get; private set; }
    public long StickerDocumentId { get; private set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<StarGiftAggregate, StarGiftId, StarGiftCreatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Id = domainEvent.AggregateIdentity.Value;
        GiftId = e.GiftId;
        Stars = e.Stars;
        ConvertStars = e.ConvertStars;
        AvailabilityTotal = e.AvailabilityTotal;
        AvailabilityRemains = e.AvailabilityTotal;
        Limited = e.AvailabilityTotal > 0;
        StickerDocumentId = e.StickerDocumentId;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<StarGiftAggregate, StarGiftId, StarGiftAvailabilityUpdatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        AvailabilityRemains = e.AvailabilityRemains;
        SoldOut = e.AvailabilityRemains <= 0 && Limited;
        return Task.CompletedTask;
    }
}
