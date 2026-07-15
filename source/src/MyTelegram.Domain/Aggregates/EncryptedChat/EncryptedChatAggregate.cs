namespace MyTelegram.Domain.Aggregates.EncryptedChat;

[EnableAutoGeneration]
public class EncryptedChatAggregate : AggregateRoot<EncryptedChatAggregate, EncryptedChatId>
{
    private readonly EncryptedChatState _state = new();

    public EncryptedChatAggregate(EncryptedChatId id) : base(id)
    {
        Register(_state);
    }

    public void RequestEncryptedChat(int chatId, long accessHash, long adminId, long participantId,
        byte[] ga, long adminPermAuthKeyId, int date)
    {
        if (IsNew)
            Emit(new EncryptedChatRequestedEvent(chatId, accessHash, adminId, participantId, ga, adminPermAuthKeyId, date));
    }

    public void AcceptEncryptedChat(byte[] gb, long keyFingerprint, long participantPermAuthKeyId)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        if (_state.ChatState != "requested")
            throw DomainError.With("Encryption already accepted or declined");
        Emit(new EncryptedChatAcceptedEvent(gb, keyFingerprint, participantPermAuthKeyId));
    }

    public void DiscardEncryptedChat(bool deleteHistory)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new EncryptedChatDiscardedEvent(deleteHistory));
    }
}
