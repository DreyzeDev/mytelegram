namespace MyTelegram.Domain.Aggregates.Channel;

[EnableAutoGeneration]
public class ForumTopicAggregate : AggregateRoot<ForumTopicAggregate, ForumTopicId>
{
    private readonly ForumTopicState _state = new();

    public ForumTopicAggregate(ForumTopicId id) : base(id)
    {
        Register(_state);
    }

    public void CreateTopic(long channelId, int topicId, long creatorUserId, string title,
        int? iconColor, long? iconEmojiId, int date)
    {
        if (IsNew)
            Emit(new ForumTopicCreatedEvent(channelId, topicId, creatorUserId, title, iconColor, iconEmojiId, date));
    }

    public void EditTopic(long channelId, int topicId, string? title, long? iconEmojiId,
        bool? closed, bool? hidden, bool? pinned)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new ForumTopicEditedEvent(channelId, topicId, title, iconEmojiId, closed, hidden, pinned));
    }
}
