namespace MyTelegram.Domain.Aggregates.FaveSticker;

[EnableAutoGeneration]
public class FaveStickerAggregate : AggregateRoot<FaveStickerAggregate, FaveStickerId>
{
    private readonly FaveStickerState _state = new();

    public FaveStickerAggregate(FaveStickerId id) : base(id)
    {
        Register(_state);
    }

    public void Fave(long userId, long documentId, int date)
    {
        Emit(new FaveStickerEvent(userId, documentId, date));
    }

    public void Unfave(long userId, long documentId)
    {
        if (!IsNew && _state.Faved)
        {
            Emit(new UnfaveStickerEvent(userId, documentId));
        }
    }
}
