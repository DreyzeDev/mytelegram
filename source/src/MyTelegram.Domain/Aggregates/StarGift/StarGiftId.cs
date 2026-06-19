namespace MyTelegram.Domain.Aggregates.StarGift;

public class StarGiftId(string value) : Identity<StarGiftId>(value)
{
    public static StarGiftId Create(long giftId) =>
        NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"stargift-{giftId}");
}
