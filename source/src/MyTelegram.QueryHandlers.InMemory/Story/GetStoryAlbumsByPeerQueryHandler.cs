namespace MyTelegram.QueryHandlers.InMemory.Story;

public class GetStoryAlbumsByPeerQueryHandler(IQueryOnlyReadModelStore<StoryAlbumReadModel> store)
    : IQueryHandler<GetStoryAlbumsByPeerQuery, IReadOnlyCollection<IStoryAlbumReadModel>>
{
    public async Task<IReadOnlyCollection<IStoryAlbumReadModel>> ExecuteQueryAsync(GetStoryAlbumsByPeerQuery query, CancellationToken cancellationToken)
        => await store.FindAsync(p => p.OwnerPeerId == query.OwnerPeerId, cancellationToken: cancellationToken);
}
