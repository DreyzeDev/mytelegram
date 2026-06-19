namespace MyTelegram.Domain.Aggregates.StickerSet;

public class StickerSetId(string value) : Identity<StickerSetId>(value)
{
    public static StickerSetId Create(long stickerSetId)
        => NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"stickersetid-{stickerSetId}");
}
