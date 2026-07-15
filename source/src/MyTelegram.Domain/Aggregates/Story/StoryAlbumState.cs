namespace MyTelegram.Domain.Aggregates.Story;

public class StoryAlbumState : AggregateState<StoryAlbumAggregate, StoryAlbumId, StoryAlbumState>,
    IApply<StoryAlbumCreatedEvent>,
    IApply<StoryAlbumUpdatedEvent>,
    IApply<StoryAlbumDeletedEvent>
{
    public long OwnerPeerId { get; private set; }
    public int AlbumId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public List<int> StoryIds { get; private set; } = [];

    public void Apply(StoryAlbumCreatedEvent e)
    {
        OwnerPeerId = e.OwnerPeerId;
        AlbumId = e.AlbumId;
        Title = e.Title;
        StoryIds = new List<int>(e.StoryIds);
    }

    public void Apply(StoryAlbumUpdatedEvent e)
    {
        if (e.Title != null) Title = e.Title;
        if (e.AddStories != null)
            foreach (var id in e.AddStories.Where(id => !StoryIds.Contains(id)))
                StoryIds.Add(id);
        if (e.DeleteStories != null)
            StoryIds.RemoveAll(id => e.DeleteStories.Contains(id));
        if (e.Order != null)
            StoryIds = e.Order.Where(id => StoryIds.Contains(id)).ToList();
    }

    public void Apply(StoryAlbumDeletedEvent e) { }
}
