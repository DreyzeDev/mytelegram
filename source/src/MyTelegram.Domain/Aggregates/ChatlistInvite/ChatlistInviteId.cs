namespace MyTelegram.Domain.Aggregates.ChatlistInvite;

public class ChatlistInviteId(string value) : Identity<ChatlistInviteId>(value)
{
    public static ChatlistInviteId Create(long userId, string slug) =>
        NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"cli-{userId}-{slug}");
}
