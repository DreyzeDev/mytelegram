namespace MyTelegram.ReadModel.Impl;

public class UserStarGiftReadModel : ReadModelBase, IUserStarGiftReadModel,
    IAmReadModelFor<UserStarGiftAggregate, UserStarGiftId, UserStarGiftReceivedEvent>,
    IAmReadModelFor<UserStarGiftAggregate, UserStarGiftId, UserStarGiftSavedEvent>,
    IAmReadModelFor<UserStarGiftAggregate, UserStarGiftId, UserStarGiftConvertedEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }
    public long OwnerPeerId { get; private set; }
    public int MsgId { get; private set; }
    public long GiftId { get; private set; }
    public long SenderPeerId { get; private set; }
    public int Date { get; private set; }
    public long ConvertStars { get; private set; }
    public bool Unsaved { get; private set; }
    public bool Converted { get; private set; }
    public bool NameHidden { get; private set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<UserStarGiftAggregate, UserStarGiftId, UserStarGiftReceivedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Id = domainEvent.AggregateIdentity.Value;
        OwnerPeerId = e.OwnerPeerId;
        MsgId = e.MsgId;
        GiftId = e.GiftId;
        SenderPeerId = e.SenderPeerId;
        Date = e.Date;
        ConvertStars = e.ConvertStars;
        NameHidden = e.NameHidden;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<UserStarGiftAggregate, UserStarGiftId, UserStarGiftSavedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Unsaved = domainEvent.AggregateEvent.Unsaved;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<UserStarGiftAggregate, UserStarGiftId, UserStarGiftConvertedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Converted = true;
        return Task.CompletedTask;
    }
}
