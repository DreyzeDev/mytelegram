namespace MyTelegram.Domain.Aggregates.StarGift;

public class UserStarGiftId(string value) : Identity<UserStarGiftId>(value)
{
    public static UserStarGiftId Create(long ownerPeerId, int msgId) =>
        NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"userstargift-{ownerPeerId}-{msgId}");
}
