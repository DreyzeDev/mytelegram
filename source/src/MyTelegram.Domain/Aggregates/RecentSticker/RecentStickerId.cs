namespace MyTelegram.Domain.Aggregates.RecentSticker;

public class RecentStickerId(string value) : Identity<RecentStickerId>(value)
{
    public static RecentStickerId Create(long userId, long documentId, bool attached)
        => NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands,
            $"recent-sticker-{userId}-{documentId}-{attached}");
}
