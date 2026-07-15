namespace MyTelegram.Domain.Aggregates.RecentSticker;

[EnableAutoGeneration]
public class RecentStickerAggregate : AggregateRoot<RecentStickerAggregate, RecentStickerId>
{
    private readonly RecentStickerState _state = new();

    public RecentStickerAggregate(RecentStickerId id) : base(id)
    {
        Register(_state);
    }

    public void Save(long userId, long documentId, bool attached, int date)
    {
        Emit(new SaveRecentStickerEvent(userId, documentId, attached, date));
    }

    public void Unsave(long userId, long documentId, bool attached)
    {
        if (!IsNew && _state.Saved)
        {
            Emit(new UnsaveRecentStickerEvent(userId, documentId, attached));
        }
    }
}
