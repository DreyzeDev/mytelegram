namespace MyTelegram.ReadModel.Interfaces;

public interface IStarGiftReadModel : IReadModel
{
    long GiftId { get; }
    long Stars { get; }
    long ConvertStars { get; }
    int AvailabilityTotal { get; }
    int AvailabilityRemains { get; }
    bool Limited { get; }
    bool SoldOut { get; }
    long StickerDocumentId { get; }
}
