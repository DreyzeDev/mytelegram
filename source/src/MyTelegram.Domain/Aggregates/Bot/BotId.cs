namespace MyTelegram.Domain.Aggregates.Bot;

public class BotId(string value) : Identity<BotId>(value)
{
    public static BotId Create(long botUserId) =>
        NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"bot-{botUserId}");
}
