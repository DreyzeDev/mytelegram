namespace MyTelegram.Domain.Aggregates.Channel;

public class ForumTopicState : AggregateState<ForumTopicAggregate, ForumTopicId, ForumTopicState>,
    IApply<ForumTopicCreatedEvent>,
    IApply<ForumTopicEditedEvent>
{
    public long ChannelId { get; private set; }
    public int TopicId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public bool Closed { get; private set; }
    public bool Hidden { get; private set; }
    public bool Pinned { get; private set; }

    public void Apply(ForumTopicCreatedEvent e)
    {
        ChannelId = e.ChannelId;
        TopicId = e.TopicId;
        Title = e.Title;
    }

    public void Apply(ForumTopicEditedEvent e)
    {
        if (e.Title != null) Title = e.Title;
        if (e.Closed.HasValue) Closed = e.Closed.Value;
        if (e.Hidden.HasValue) Hidden = e.Hidden.Value;
        if (e.Pinned.HasValue) Pinned = e.Pinned.Value;
    }
}
