namespace MyTelegram.Domain.Aggregates.StickerSet;

public class StickerSetState : AggregateState<StickerSetAggregate, StickerSetId, StickerSetState>,
    IApply<StickerSetCreatedEvent>
{
    public long StickerSetId { get; private set; }

    public void Apply(StickerSetCreatedEvent aggregateEvent)
    {
        StickerSetId = aggregateEvent.StickerSetId;
    }
}
