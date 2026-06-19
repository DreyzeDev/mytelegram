namespace MyTelegram.QueryHandlers.InMemory.ForumTopic;

public class GetForumTopicByIdQueryHandler(IQueryOnlyReadModelStore<ForumTopicReadModel> store)
    : IQueryHandler<GetForumTopicByIdQuery, IForumTopicReadModel?>
{
    public async Task<IForumTopicReadModel?> ExecuteQueryAsync(GetForumTopicByIdQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.ChannelId == query.ChannelId && p.TopicId == query.TopicId, cancellationToken);
}
