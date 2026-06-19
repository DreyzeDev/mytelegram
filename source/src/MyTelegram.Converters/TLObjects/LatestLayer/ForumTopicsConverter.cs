namespace MyTelegram.Converters.TLObjects.LatestLayer;

internal sealed class ForumTopicsConverter : IForumTopicsConverter, ITransientDependency
{
    public int Layer => Layers.LayerLatest;

    public IForumTopic ToForumTopic(IForumTopicReadModel readModel)
    {
        return new TForumTopic
        {
            Id = readModel.TopicId,
            Date = readModel.Date,
            Title = readModel.Title,
            IconColor = readModel.IconColor ?? 0x6FB9F0,
            IconEmojiId = readModel.IconEmojiId,
            TopMessage = readModel.TopMessage,
            ReadInboxMaxId = readModel.ReadInboxMaxId,
            ReadOutboxMaxId = readModel.ReadOutboxMaxId,
            UnreadCount = readModel.UnreadCount,
            UnreadMentionsCount = readModel.UnreadMentionsCount,
            UnreadReactionsCount = readModel.UnreadReactionsCount,
            Pinned = readModel.Pinned,
            Closed = readModel.Closed,
            Hidden = readModel.Hidden,
            Peer = new TPeerChannel { ChannelId = readModel.ChannelId },
            FromId = new TPeerUser { UserId = readModel.CreatorUserId },
            NotifySettings = new TPeerNotifySettings()
        };
    }
}
