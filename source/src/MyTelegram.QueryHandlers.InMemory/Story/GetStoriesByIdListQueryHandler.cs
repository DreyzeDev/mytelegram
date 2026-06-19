namespace MyTelegram.QueryHandlers.InMemory.Story;

public class GetStoriesByIdListQueryHandler(IQueryOnlyReadModelStore<StoryReadModel> store)
    : IQueryHandler<GetStoriesByIdListQuery, IReadOnlyCollection<IStoryReadModel>>
{
    public async Task<IReadOnlyCollection<IStoryReadModel>> ExecuteQueryAsync(GetStoriesByIdListQuery query, CancellationToken cancellationToken)
        => await store.FindAsync(p => p.OwnerPeerId == query.PeerId && query.StoryIds.Contains(p.StoryId), cancellationToken: cancellationToken);
}
