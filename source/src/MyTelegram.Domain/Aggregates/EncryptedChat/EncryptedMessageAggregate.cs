namespace MyTelegram.Domain.Aggregates.EncryptedChat;

[EnableAutoGeneration]
public class EncryptedMessageAggregate : AggregateRoot<EncryptedMessageAggregate, EncryptedMessageId>
{
    private readonly EncryptedMessageState _state = new();

    public EncryptedMessageAggregate(EncryptedMessageId id) : base(id)
    {
        Register(_state);
    }

    public void SendEncryptedMessage(int chatId, long userId, long permAuthKeyId,
        byte[] data, byte[]? file, int qts, long randomId, SendMessageType messageType, int date)
    {
        if (IsNew)
            Emit(new EncryptedMessageSentEvent(chatId, userId, permAuthKeyId, data, file, qts, randomId, messageType, date));
    }
}
