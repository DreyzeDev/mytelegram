namespace MyTelegram.QueryHandlers.InMemory.Story;

public class GetStoryAlbumByIdQueryHandler(IQueryOnlyReadModelStore<StoryAlbumReadModel> store)
    : IQueryHandler<GetStoryAlbumByIdQuery, IStoryAlbumReadModel?>
{
    public async Task<IStoryAlbumReadModel?> ExecuteQueryAsync(GetStoryAlbumByIdQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.OwnerPeerId == query.OwnerPeerId && p.AlbumId == query.AlbumId, cancellationToken);
}
