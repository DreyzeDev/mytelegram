namespace MyTelegram.Domain.Aggregates.EncryptedChat;

public class EncryptedMessageId(string value) : Identity<EncryptedMessageId>(value)
{
    public static EncryptedMessageId Create(long randomId) =>
        NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"encryptedmessage-{randomId}");
}
