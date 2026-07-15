namespace MyTelegram.Domain.Aggregates.Story;

public class StoryState : AggregateState<StoryAggregate, StoryId, StoryState>,
    IApply<StoryCreatedEvent>,
    IApply<StoryDeletedEvent>,
    IApply<StoryEditedEvent>,
    IApply<StoryPinnedToggledEvent>,
    IApply<StoryViewIncrementedEvent>
{
    public long OwnerPeerId { get; private set; }
    public int StoryId { get; private set; }
    public bool Pinned { get; private set; }
    public int ViewsCount { get; private set; }
    public List<long> RecentViewers { get; private set; } = [];

    public void Apply(StoryCreatedEvent e)
    {
        OwnerPeerId = e.StoryItem.Peer.PeerId;
        StoryId = e.StoryItem.Id;
        Pinned = e.StoryItem.Pinned;
    }

    public void Apply(StoryDeletedEvent e) { }

    public void Apply(StoryEditedEvent e)
    {
        Pinned = e.StoryItem.Pinned;
    }

    public void Apply(StoryPinnedToggledEvent e)
    {
        Pinned = e.Pinned;
    }

    public void Apply(StoryViewIncrementedEvent e)
    {
        ViewsCount++;
        if (!RecentViewers.Contains(e.ViewerUserId))
            RecentViewers.Add(e.ViewerUserId);
        if (RecentViewers.Count > 20)
            RecentViewers.RemoveAt(0);
    }
}
