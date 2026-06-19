namespace MyTelegram.Domain.Aggregates.BusinessChatLink;

public class BusinessChatLinkId(string value) : Identity<BusinessChatLinkId>(value)
{
    public static BusinessChatLinkId Create(long userId, string slug) =>
        NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"bcl-{userId}-{slug}");
}
