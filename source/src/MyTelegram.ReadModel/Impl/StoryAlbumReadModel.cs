namespace MyTelegram.ReadModel.Impl;

public class StoryAlbumReadModel : ReadModelBase, IStoryAlbumReadModel,
    IAmReadModelFor<StoryAlbumAggregate, StoryAlbumId, StoryAlbumCreatedEvent>,
    IAmReadModelFor<StoryAlbumAggregate, StoryAlbumId, StoryAlbumUpdatedEvent>,
    IAmReadModelFor<StoryAlbumAggregate, StoryAlbumId, StoryAlbumDeletedEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }
    public long OwnerPeerId { get; private set; }
    public int AlbumId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public List<int> StoryIds { get; private set; } = [];

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<StoryAlbumAggregate, StoryAlbumId, StoryAlbumCreatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Id = domainEvent.AggregateIdentity.Value;
        var e = domainEvent.AggregateEvent;
        OwnerPeerId = e.OwnerPeerId;
        AlbumId = e.AlbumId;
        Title = e.Title;
        StoryIds = new List<int>(e.StoryIds);
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<StoryAlbumAggregate, StoryAlbumId, StoryAlbumUpdatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        if (e.Title != null) Title = e.Title;
        if (e.AddStories != null)
            foreach (var id in e.AddStories.Where(id => !StoryIds.Contains(id)))
                StoryIds.Add(id);
        if (e.DeleteStories != null)
            StoryIds.RemoveAll(id => e.DeleteStories.Contains(id));
        if (e.Order != null)
            StoryIds = e.Order.Where(id => StoryIds.Contains(id)).ToList();
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<StoryAlbumAggregate, StoryAlbumId, StoryAlbumDeletedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        context.MarkForDeletion();
        return Task.CompletedTask;
    }
}
