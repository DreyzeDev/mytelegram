namespace MyTelegram.Domain.Aggregates.StickerSet;

[EnableAutoGeneration]
public class StickerSetAggregate : AggregateRoot<StickerSetAggregate, StickerSetId>
{
    private readonly StickerSetState _state = new();

    public StickerSetAggregate(StickerSetId id) : base(id)
    {
        Register(_state);
    }

    public void CreateStickerSet(
        long stickerSetId,
        long accessHash,
        string title,
        string shortName,
        StickerSetType stickerSetType,
        bool masks,
        bool emojis,
        bool textColor,
        bool channelEmojiStatus,
        List<StickerPackItem> packs,
        List<StickerKeywordItem> keywords,
        List<long> stickerDocumentIds,
        List<long> covers,
        int count,
        List<PhotoSize>? thumbs,
        int? thumbVersion,
        long? thumbDocumentId,
        long creatorUserId = 0,
        bool featured = false)
    {
        if (IsNew)
        {
            Emit(new StickerSetCreatedEvent(stickerSetId, accessHash, title, shortName, stickerSetType,
                masks, emojis, textColor, channelEmojiStatus, packs, keywords, stickerDocumentIds,
                covers, count, thumbs, thumbVersion, thumbDocumentId, creatorUserId, featured));
        }
    }
}
