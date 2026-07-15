namespace MyTelegram.QueryHandlers.InMemory.Story;

public class GetArchivedStoriesQueryHandler(IQueryOnlyReadModelStore<StoryReadModel> store)
    : IQueryHandler<GetArchivedStoriesQuery, IReadOnlyCollection<IStoryReadModel>>
{
    public async Task<IReadOnlyCollection<IStoryReadModel>> ExecuteQueryAsync(GetArchivedStoriesQuery query, CancellationToken cancellationToken)
    {
        var results = await store.FindAsync(p => p.OwnerPeerId == query.PeerId, cancellationToken: cancellationToken);

        if (query.OffsetId > 0)
            results = results.Where(p => p.StoryId < query.OffsetId).ToList();

        return results
            .OrderByDescending(p => p.StoryId)
            .Take(query.Limit > 0 ? query.Limit : 100)
            .ToList();
    }
}
