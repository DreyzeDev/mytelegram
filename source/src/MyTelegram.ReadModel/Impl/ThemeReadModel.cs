namespace MyTelegram.ReadModel.Impl;

public class ThemeReadModel : ReadModelBase, IThemeReadModel,
    IAmReadModelFor<ThemeAggregate, ThemeId, ThemeCreatedEvent>,
    IAmReadModelFor<ThemeAggregate, ThemeId, ThemeUpdatedEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }
    public long CreatorUserId { get; private set; }
    public long ThemeId { get; private set; }
    public string Slug { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public long? DocumentId { get; private set; }
    public List<ThemeSettings> Settings { get; private set; } = [];
    public string? Emoticon { get; private set; }
    public string Format { get; private set; } = null!;
    Theme IThemeReadModel.Theme => new(false, false, ThemeId, 0, Slug, Title, DocumentId, Settings, Emoticon, Format);

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<ThemeAggregate, ThemeId, ThemeCreatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Id = domainEvent.AggregateIdentity.Value;
        CreatorUserId = e.CreatorUserId;
        ThemeId = e.ThemeId;
        Slug = e.Slug;
        Title = e.Title;
        DocumentId = e.DocumentId;
        Settings = e.Settings;
        Emoticon = e.Emoticon;
        Format = e.Format;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<ThemeAggregate, ThemeId, ThemeUpdatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        if (e.Slug != null) Slug = e.Slug;
        if (e.Title != null) Title = e.Title;
        if (e.DocumentId.HasValue) DocumentId = e.DocumentId;
        if (e.Settings != null) Settings = e.Settings;
        if (e.Emoticon != null) Emoticon = e.Emoticon;
        return Task.CompletedTask;
    }
}
