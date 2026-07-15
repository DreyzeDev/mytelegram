namespace MyTelegram.Domain.Aggregates.RecentSticker;

public class RecentStickerState : AggregateState<RecentStickerAggregate, RecentStickerId, RecentStickerState>,
    IApply<SaveRecentStickerEvent>,
    IApply<UnsaveRecentStickerEvent>
{
    public bool Saved { get; private set; }

    public void Apply(SaveRecentStickerEvent aggregateEvent)
    {
        Saved = true;
    }

    public void Apply(UnsaveRecentStickerEvent aggregateEvent)
    {
        Saved = false;
    }
}
