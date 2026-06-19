namespace MyTelegram.QueryHandlers.MongoDB.Story;

public class GetStoryByIdQueryHandler(IQueryOnlyReadModelStore<StoryReadModel> store)
    : IQueryHandler<GetStoryByIdQuery, IStoryReadModel?>
{
    public async Task<IStoryReadModel?> ExecuteQueryAsync(GetStoryByIdQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.OwnerPeerId == query.PeerId && p.StoryId == query.StoryId, cancellationToken);
}
