namespace MyTelegram.Domain.Aggregates.EncryptedChat;

public class EncryptedMessageState : AggregateState<EncryptedMessageAggregate, EncryptedMessageId, EncryptedMessageState>,
    IApply<EncryptedMessageSentEvent>
{
    public int ChatId { get; private set; }
    public long UserId { get; private set; }
    public long PermAuthKeyId { get; private set; }
    public byte[] Data { get; private set; } = [];
    public byte[]? File { get; private set; }
    public int Qts { get; private set; }
    public long RandomId { get; private set; }
    public SendMessageType MessageType { get; private set; }
    public int Date { get; private set; }

    public void Apply(EncryptedMessageSentEvent e)
    {
        ChatId = e.ChatId;
        UserId = e.UserId;
        PermAuthKeyId = e.PermAuthKeyId;
        Data = e.Data;
        File = e.File;
        Qts = e.Qts;
        RandomId = e.RandomId;
        MessageType = e.MessageType;
        Date = e.Date;
    }
}
