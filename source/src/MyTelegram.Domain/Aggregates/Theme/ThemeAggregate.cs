namespace MyTelegram.Domain.Aggregates.Theme;

[EnableAutoGeneration]
public class ThemeAggregate : AggregateRoot<ThemeAggregate, ThemeId>
{
    private readonly ThemeState _state = new();

    public ThemeAggregate(ThemeId id) : base(id)
    {
        Register(_state);
    }

    public void CreateTheme(long creatorUserId, long themeId, string slug, string title,
        long? documentId, List<ThemeSettings> settings, string? emoticon, string format)
    {
        if (IsNew)
            Emit(new ThemeCreatedEvent(creatorUserId, themeId, slug, title, documentId, settings, emoticon, format));
    }

    public void UpdateTheme(string? slug, string? title, long? documentId,
        List<ThemeSettings>? settings, string? emoticon)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new ThemeUpdatedEvent(slug, title, documentId, settings, emoticon));
    }
}
