namespace MyTelegram.Domain.Aggregates.Privacy;

public class PrivacyId(string value) : Identity<PrivacyId>(value)
{
    public static PrivacyId Create(long userId, PrivacyType privacyType)
        => NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"privacy-{userId}-{(int)privacyType}");
}
