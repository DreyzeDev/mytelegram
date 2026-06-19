namespace MyTelegram.Domain.Aggregates.InstalledStickerSet;

public class InstalledStickerSetId(string value) : Identity<InstalledStickerSetId>(value)
{
    public static InstalledStickerSetId Create(long userId, long stickerSetId)
        => NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"installed-sticker-{userId}-{stickerSetId}");
}
