namespace MyTelegram.QueryHandlers.InMemory.Story;

public class GetPinnedStoriesQueryHandler(IQueryOnlyReadModelStore<StoryReadModel> store)
    : IQueryHandler<GetPinnedStoriesQuery, IReadOnlyCollection<IStoryReadModel>>
{
    public async Task<IReadOnlyCollection<IStoryReadModel>> ExecuteQueryAsync(GetPinnedStoriesQuery query, CancellationToken cancellationToken)
    {
        var results = await store.FindAsync(p => p.OwnerPeerId == query.PeerId && p.Pinned, cancellationToken: cancellationToken);

        if (query.OffsetId > 0)
            results = results.Where(p => p.StoryId < query.OffsetId).ToList();

        return results
            .OrderByDescending(p => p.StoryId)
            .Take(query.Limit > 0 ? query.Limit : 100)
            .ToList();
    }
}
