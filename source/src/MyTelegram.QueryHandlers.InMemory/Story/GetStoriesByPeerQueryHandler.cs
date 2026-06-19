namespace MyTelegram.QueryHandlers.InMemory.Story;

public class GetStoriesByPeerQueryHandler(IQueryOnlyReadModelStore<StoryReadModel> store)
    : IQueryHandler<GetStoriesByPeerQuery, IReadOnlyCollection<IStoryReadModel>>
{
    public async Task<IReadOnlyCollection<IStoryReadModel>> ExecuteQueryAsync(GetStoriesByPeerQuery query, CancellationToken cancellationToken)
        => await store.FindAsync(p => p.OwnerPeerId == query.PeerId, cancellationToken: cancellationToken);
}
