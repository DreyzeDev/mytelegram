namespace MyTelegram.Domain.Aggregates.EncryptedChat;

public class EncryptedChatId(string value) : Identity<EncryptedChatId>(value)
{
    public static EncryptedChatId Create(int chatId) =>
        NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"encryptedchat-{chatId}");
}
