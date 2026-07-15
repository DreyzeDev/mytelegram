namespace MyTelegram.QueryHandlers.MongoDB.Story;

public class GetActiveStoriesQueryHandler(IQueryOnlyReadModelStore<StoryReadModel> store)
    : IQueryHandler<GetActiveStoriesQuery, IReadOnlyCollection<IStoryReadModel>>
{
    public async Task<IReadOnlyCollection<IStoryReadModel>> ExecuteQueryAsync(GetActiveStoriesQuery query, CancellationToken cancellationToken)
    {
        var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return await store.FindAsync(p => p.OwnerPeerId == query.PeerId && p.ExpireDate > now, cancellationToken: cancellationToken);
    }
}
