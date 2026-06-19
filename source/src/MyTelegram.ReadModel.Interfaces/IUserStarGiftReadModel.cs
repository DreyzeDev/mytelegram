namespace MyTelegram.ReadModel.Interfaces;

public interface IUserStarGiftReadModel : IReadModel
{
    long OwnerPeerId { get; }
    int MsgId { get; }
    long GiftId { get; }
    long SenderPeerId { get; }
    int Date { get; }
    long ConvertStars { get; }
    bool Unsaved { get; }
    bool Converted { get; }
    bool NameHidden { get; }
}
