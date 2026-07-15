namespace MyTelegram.ReadModel.Impl;

public class EncryptedMessageReadModel : ReadModelBase, IEncryptedMessageReadModel,
    IAmReadModelFor<EncryptedMessageAggregate, EncryptedMessageId, EncryptedMessageSentEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }
    public long ChatId { get; private set; }
    public long UserId { get; private set; }
    public long PermAuthKeyId { get; private set; }
    public byte[] Data { get; private set; } = [];
    public byte[]? File { get; private set; }
    public int Date { get; private set; }
    public SendMessageType MessageType { get; private set; }
    public int Qts { get; private set; }
    public long RandomId { get; private set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<EncryptedMessageAggregate, EncryptedMessageId, EncryptedMessageSentEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Id = domainEvent.AggregateIdentity.Value;
        var e = domainEvent.AggregateEvent;
        ChatId = e.ChatId;
        UserId = e.UserId;
        PermAuthKeyId = e.PermAuthKeyId;
        Data = e.Data;
        File = e.File;
        Date = e.Date;
        MessageType = e.MessageType;
        Qts = e.Qts;
        RandomId = e.RandomId;
        return Task.CompletedTask;
    }
}
