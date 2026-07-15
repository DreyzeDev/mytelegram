namespace MyTelegram.Domain.Aggregates.FaveSticker;

public class FaveStickerState : AggregateState<FaveStickerAggregate, FaveStickerId, FaveStickerState>,
    IApply<FaveStickerEvent>,
    IApply<UnfaveStickerEvent>
{
    public bool Faved { get; private set; }

    public void Apply(FaveStickerEvent aggregateEvent)
    {
        Faved = true;
    }

    public void Apply(UnfaveStickerEvent aggregateEvent)
    {
        Faved = false;
    }
}
