namespace MyTelegram.QueryHandlers.InMemory.ForumTopic;

public class GetForumTopicsByIdsQueryHandler(IQueryOnlyReadModelStore<ForumTopicReadModel> store)
    : IQueryHandler<GetForumTopicsByIdsQuery, IReadOnlyCollection<IForumTopicReadModel>>
{
    public Task<IReadOnlyCollection<IForumTopicReadModel>> ExecuteQueryAsync(GetForumTopicsByIdsQuery query, CancellationToken cancellationToken)
        => store.FindAsync(
            p => p.ChannelId == query.ChannelId && query.TopicIds.Contains(p.TopicId),
            p => (IForumTopicReadModel)p,
            cancellationToken: cancellationToken);
}
