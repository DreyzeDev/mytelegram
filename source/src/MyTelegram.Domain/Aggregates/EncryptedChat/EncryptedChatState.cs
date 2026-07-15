namespace MyTelegram.Domain.Aggregates.EncryptedChat;

public class EncryptedChatState : AggregateState<EncryptedChatAggregate, EncryptedChatId, EncryptedChatState>,
    IApply<EncryptedChatRequestedEvent>,
    IApply<EncryptedChatAcceptedEvent>,
    IApply<EncryptedChatDiscardedEvent>
{
    public int ChatId { get; private set; }
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

    public void Apply(EncryptedChatRequestedEvent e)
    {
        ChatId = e.ChatId;
        AccessHash = e.AccessHash;
        AdminId = e.AdminId;
        ParticipantId = e.ParticipantId;
        Ga = e.Ga;
        AdminPermAuthKeyId = e.AdminPermAuthKeyId;
        Date = e.Date;
        ChatState = "requested";
    }

    public void Apply(EncryptedChatAcceptedEvent e)
    {
        Gb = e.Gb;
        KeyFingerprint = e.KeyFingerprint;
        ParticipantPermAuthKeyId = e.ParticipantPermAuthKeyId;
        ChatState = "accepted";
    }

    public void Apply(EncryptedChatDiscardedEvent e)
    {
        ChatState = "discarded";
    }
}
