namespace MyTelegram.Domain.Aggregates.StarGift;

public class StarGiftState : AggregateState<StarGiftAggregate, StarGiftId, StarGiftState>,
    IApply<StarGiftCreatedEvent>,
    IApply<StarGiftAvailabilityUpdatedEvent>
{
    public long GiftId { get; private set; }
    public long Stars { get; private set; }
    public long ConvertStars { get; private set; }
    public int AvailabilityTotal { get; private set; }
    public int AvailabilityRemains { get; private set; }
    public bool Limited { get; private set; }
    public bool SoldOut { get; private set; }
    public long StickerDocumentId { get; private set; }

    public void Apply(StarGiftCreatedEvent e)
    {
        GiftId = e.GiftId;
        Stars = e.Stars;
        ConvertStars = e.ConvertStars;
        AvailabilityTotal = e.AvailabilityTotal;
        AvailabilityRemains = e.AvailabilityTotal;
        Limited = e.AvailabilityTotal > 0;
        StickerDocumentId = e.StickerDocumentId;
    }

    public void Apply(StarGiftAvailabilityUpdatedEvent e)
    {
        AvailabilityRemains = e.AvailabilityRemains;
        SoldOut = e.AvailabilityRemains <= 0 && Limited;
    }
}
