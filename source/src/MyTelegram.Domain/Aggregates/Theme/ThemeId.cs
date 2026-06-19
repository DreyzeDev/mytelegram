namespace MyTelegram.Domain.Aggregates.Theme;

public class ThemeId(string value) : Identity<ThemeId>(value)
{
    public static ThemeId Create(long creatorUserId, string slug) =>
        NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"theme-{creatorUserId}-{slug}");
}
