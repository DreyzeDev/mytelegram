namespace MyTelegram.QueryHandlers.InMemory.ForumTopic;

public class GetForumTopicsQueryHandler(IQueryOnlyReadModelStore<ForumTopicReadModel> store)
    : IQueryHandler<GetForumTopicsQuery, IReadOnlyCollection<IForumTopicReadModel>>
{
    public Task<IReadOnlyCollection<IForumTopicReadModel>> ExecuteQueryAsync(GetForumTopicsQuery query, CancellationToken cancellationToken)
        => store.FindAsync(
            p => p.ChannelId == query.ChannelId &&
                 (query.Q == null || p.Title.Contains(query.Q)) &&
                 (query.OffsetTopic == 0 || p.TopicId < query.OffsetTopic),
            p => (IForumTopicReadModel)p,
            limit: query.Limit > 0 ? query.Limit : 100,
            sort: new SortOptions<ForumTopicReadModel>(p => p.TopicId, SortType.Descending),
            cancellationToken: cancellationToken);
}
