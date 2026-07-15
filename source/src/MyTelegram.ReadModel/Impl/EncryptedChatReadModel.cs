namespace MyTelegram.ReadModel.Impl;

public class EncryptedChatReadModel : ReadModelBase, IEncryptedChatReadModel,
    IAmReadModelFor<EncryptedChatAggregate, EncryptedChatId, EncryptedChatRequestedEvent>,
    IAmReadModelFor<EncryptedChatAggregate, EncryptedChatId, EncryptedChatAcceptedEvent>,
    IAmReadModelFor<EncryptedChatAggregate, EncryptedChatId, EncryptedChatDiscardedEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }
    public long ChatId { get; private set; }
    public long RandomId { get; private set; }
    public long AccessHash { get; private set; }
    public long AdminId { get; private set; }
    public long ParticipantId { get; private set; }
    public byte[] Ga { get; private set; } = [];
    public byte[] Gb { get; private set; } = [];
    public long KeyFingerprint { get; private set; }
    public long AdminPermAuthKeyId { get; private set; }
    public long ParticipantPermAuthKeyId { get; private set; }
    public int Date { get; private set; }
    public string ChatState { get; private set; } = "requested";

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<EncryptedChatAggregate, EncryptedChatId, EncryptedChatRequestedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Id = domainEvent.AggregateIdentity.Value;
        var e = domainEvent.AggregateEvent;
        ChatId = e.ChatId;
        RandomId = e.ChatId;
        AccessHash = e.AccessHash;
        AdminId = e.AdminId;
        ParticipantId = e.ParticipantId;
        Ga = e.Ga;
        AdminPermAuthKeyId = e.AdminPermAuthKeyId;
        Date = e.Date;
        ChatState = "requested";
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<EncryptedChatAggregate, EncryptedChatId, EncryptedChatAcceptedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Gb = e.Gb;
        KeyFingerprint = e.KeyFingerprint;
        ParticipantPermAuthKeyId = e.ParticipantPermAuthKeyId;
        ChatState = "accepted";
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<EncryptedChatAggregate, EncryptedChatId, EncryptedChatDiscardedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        ChatState = "discarded";
        return Task.CompletedTask;
    }
}
