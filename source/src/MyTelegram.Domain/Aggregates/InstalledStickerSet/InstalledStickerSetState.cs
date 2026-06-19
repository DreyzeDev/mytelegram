namespace MyTelegram.Domain.Aggregates.InstalledStickerSet;

public class InstalledStickerSetState : AggregateState<InstalledStickerSetAggregate, InstalledStickerSetId, InstalledStickerSetState>,
    IApply<InstallStickerSetEvent>,
    IApply<UninstallStickerSetEvent>
{
    public bool Installed { get; private set; }

    public void Apply(InstallStickerSetEvent aggregateEvent)
    {
        Installed = true;
    }

    public void Apply(UninstallStickerSetEvent aggregateEvent)
    {
        Installed = false;
    }
}
