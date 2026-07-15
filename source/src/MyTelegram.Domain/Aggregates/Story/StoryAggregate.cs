namespace MyTelegram.Domain.Aggregates.Story;

[EnableAutoGeneration]
public class StoryAggregate : AggregateRoot<StoryAggregate, StoryId>
{
    private readonly StoryState _state = new();

    public StoryAggregate(StoryId id) : base(id)
    {
        Register(_state);
    }

    public void CreateStory(StoryItem storyItem)
    {
        if (IsNew)
            Emit(new StoryCreatedEvent(storyItem));
    }

    public void EditStory(StoryItem storyItem)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new StoryEditedEvent(storyItem));
    }

    public void DeleteStory()
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new StoryDeletedEvent());
    }

    public void TogglePinned(bool pinned)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new StoryPinnedToggledEvent(pinned));
    }

    public void IncrementView(long viewerUserId, int date)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new StoryViewIncrementedEvent(viewerUserId, date));
    }
}
