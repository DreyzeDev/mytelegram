namespace MyTelegram.ReadModel.Impl;

public class StickerSetReadModel : ReadModelBase, IStickerSetReadModel,
    IAmReadModelFor<StickerSetAggregate, StickerSetId, StickerSetCreatedEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }

    public long StickerSetId { get; private set; }
    public long AccessHash { get; private set; }
    public string ShortName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public StickerSetType StickerSetType { get; private set; }
    public bool Masks { get; private set; }
    public bool Emojis { get; private set; }
    public bool TextColor { get; private set; }
    public bool ChannelEmojiStatus { get; private set; }
    public List<PhotoSize>? Thumbs { get; private set; }
    public int? ThumbVersion { get; private set; }
    public long? ThumbDocumentId { get; private set; }
    public int Count { get; private set; }
    public List<StickerPackItem> Packs { get; private set; } = [];
    public List<StickerKeywordItem> Keywords { get; private set; } = [];
    public List<long> StickerDocumentIds { get; private set; } = [];
    public List<long> Covers { get; private set; } = [];

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<StickerSetAggregate, StickerSetId, StickerSetCreatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Id = domainEvent.AggregateIdentity.Value;
        StickerSetId = e.StickerSetId;
        AccessHash = e.AccessHash;
        Title = e.Title;
        ShortName = e.ShortName;
        StickerSetType = e.StickerSetType;
        Masks = e.Masks;
        Emojis = e.Emojis;
        TextColor = e.TextColor;
        ChannelEmojiStatus = e.ChannelEmojiStatus;
        Packs = e.Packs;
        Keywords = e.Keywords;
        StickerDocumentIds = e.StickerDocumentIds;
        Covers = e.Covers;
        Count = e.Count > 0 ? e.Count : e.StickerDocumentIds.Count;
        Thumbs = e.Thumbs;
        ThumbVersion = e.ThumbVersion;
        ThumbDocumentId = e.ThumbDocumentId;
        return Task.CompletedTask;
    }
}
