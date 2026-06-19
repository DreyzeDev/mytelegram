namespace MyTelegram.Domain.Aggregates.StarGift;

public class UserStarGiftState : AggregateState<UserStarGiftAggregate, UserStarGiftId, UserStarGiftState>,
    IApply<UserStarGiftReceivedEvent>,
    IApply<UserStarGiftSavedEvent>,
    IApply<UserStarGiftConvertedEvent>
{
    public long OwnerPeerId { get; private set; }
    public int MsgId { get; private set; }
    public long GiftId { get; private set; }
    public long SenderPeerId { get; private set; }
    public int Date { get; private set; }
    public long ConvertStars { get; private set; }
    public bool Unsaved { get; private set; }
    public bool Converted { get; private set; }
    public bool NameHidden { get; private set; }

    public void Apply(UserStarGiftReceivedEvent e)
    {
        OwnerPeerId = e.OwnerPeerId;
        MsgId = e.MsgId;
        GiftId = e.GiftId;
        SenderPeerId = e.SenderPeerId;
        Date = e.Date;
        ConvertStars = e.ConvertStars;
        NameHidden = e.NameHidden;
    }

    public void Apply(UserStarGiftSavedEvent e)
    {
        Unsaved = e.Unsaved;
    }

    public void Apply(UserStarGiftConvertedEvent e)
    {
        Converted = true;
    }
}
