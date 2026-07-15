namespace MyTelegram.ReadModel.Interfaces;

public interface IRecentStickerReadModel : IReadModel
{
    long UserId { get; }
    long DocumentId { get; }
    bool Attached { get; }
    int Date { get; }
}
