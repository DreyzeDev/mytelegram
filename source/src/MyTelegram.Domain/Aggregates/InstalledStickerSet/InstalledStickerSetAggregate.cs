namespace MyTelegram.Domain.Aggregates.InstalledStickerSet;

[EnableAutoGeneration]
public class InstalledStickerSetAggregate : AggregateRoot<InstalledStickerSetAggregate, InstalledStickerSetId>
{
    private readonly InstalledStickerSetState _state = new();

    public InstalledStickerSetAggregate(InstalledStickerSetId id) : base(id)
    {
        Register(_state);
    }

    public void Install(long userId, long stickerSetId, StickerSetType stickerSetType, int date)
    {
        Emit(new InstallStickerSetEvent(userId, stickerSetId, stickerSetType, date));
    }

    public void Uninstall(long userId, long stickerSetId)
    {
        if (!IsNew && _state.Installed)
        {
            Emit(new UninstallStickerSetEvent(userId, stickerSetId));
        }
    }
}
