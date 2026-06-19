namespace MyTelegram.Domain.Aggregates.Story;

public class StoryState : AggregateState<StoryAggregate, StoryId, StoryState>,
    IApply<StoryCreatedEvent>,
    IApply<StoryDeletedEvent>
{
    public long OwnerPeerId { get; private set; }
    public int StoryId { get; private set; }

    public void Apply(StoryCreatedEvent e)
    {
        OwnerPeerId = e.StoryItem.Peer.PeerId;
        StoryId = e.StoryItem.Id;
    }

    public void Apply(StoryDeletedEvent e) { }
}
