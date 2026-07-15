namespace MyTelegram.ReadModel.Interfaces;

public interface IFaveStickerReadModel : IReadModel
{
    long UserId { get; }
    long DocumentId { get; }
    int Date { get; }
}
