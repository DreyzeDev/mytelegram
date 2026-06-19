namespace MyTelegram.Domain.Aggregates.Theme;

public class ThemeState : AggregateState<ThemeAggregate, ThemeId, ThemeState>,
    IApply<ThemeCreatedEvent>,
    IApply<ThemeUpdatedEvent>
{
    public long CreatorUserId { get; private set; }
    public long ThemeId { get; private set; }
    public string Slug { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public long? DocumentId { get; private set; }
    public List<ThemeSettings> Settings { get; private set; } = [];
    public string? Emoticon { get; private set; }
    public string Format { get; private set; } = null!;

    public void Apply(ThemeCreatedEvent e)
    {
        CreatorUserId = e.CreatorUserId;
        ThemeId = e.ThemeId;
        Slug = e.Slug;
        Title = e.Title;
        DocumentId = e.DocumentId;
        Settings = e.Settings;
        Emoticon = e.Emoticon;
        Format = e.Format;
    }

    public void Apply(ThemeUpdatedEvent e)
    {
        if (e.Slug != null) Slug = e.Slug;
        if (e.Title != null) Title = e.Title;
        if (e.DocumentId.HasValue) DocumentId = e.DocumentId;
        if (e.Settings != null) Settings = e.Settings;
        if (e.Emoticon != null) Emoticon = e.Emoticon;
    }
}
