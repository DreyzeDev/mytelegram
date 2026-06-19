namespace MyTelegram.ReadModel.Impl;

public class InstalledStickerSetReadModel : ReadModelBase, IInstalledStickerSetReadModel,
    IAmReadModelFor<InstalledStickerSetAggregate, InstalledStickerSetId, InstallStickerSetEvent>,
    IAmReadModelFor<InstalledStickerSetAggregate, InstalledStickerSetId, UninstallStickerSetEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }

    public long UserId { get; private set; }
    public long StickerSetId { get; private set; }
    public bool Archived { get; private set; }
    public StickerSetType StickerSetType { get; private set; }
    public int Date { get; private set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<InstalledStickerSetAggregate, InstalledStickerSetId, InstallStickerSetEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Id = domainEvent.AggregateIdentity.Value;
        UserId = e.UserId;
        StickerSetId = e.StickerSetId;
        StickerSetType = e.StickerSetType;
        Date = e.Date;
        Archived = false;
        IsDeleted = false;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<InstalledStickerSetAggregate, InstalledStickerSetId, UninstallStickerSetEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        IsDeleted = true;
        return Task.CompletedTask;
    }
}
