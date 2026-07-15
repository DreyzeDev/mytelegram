namespace MyTelegram.Domain.Aggregates.Story;

[EnableAutoGeneration]
public class StoryAlbumAggregate : AggregateRoot<StoryAlbumAggregate, StoryAlbumId>
{
    private readonly StoryAlbumState _state = new();

    public StoryAlbumAggregate(StoryAlbumId id) : base(id)
    {
        Register(_state);
    }

    public void CreateAlbum(long ownerPeerId, int albumId, string title, List<int> storyIds)
    {
        if (IsNew)
            Emit(new StoryAlbumCreatedEvent(ownerPeerId, albumId, title, storyIds));
    }

    public void UpdateAlbum(string? title, List<int>? addStories, List<int>? deleteStories, List<int>? order)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new StoryAlbumUpdatedEvent(title, addStories, deleteStories, order));
    }

    public void DeleteAlbum()
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new StoryAlbumDeletedEvent());
    }
}
