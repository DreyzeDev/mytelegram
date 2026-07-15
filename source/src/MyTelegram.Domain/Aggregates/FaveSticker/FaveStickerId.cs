namespace MyTelegram.Domain.Aggregates.FaveSticker;

public class FaveStickerId(string value) : Identity<FaveStickerId>(value)
{
    public static FaveStickerId Create(long userId, long documentId)
        => NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"fave-sticker-{userId}-{documentId}");
}
