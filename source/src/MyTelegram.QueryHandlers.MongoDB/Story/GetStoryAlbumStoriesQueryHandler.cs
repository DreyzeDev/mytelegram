namespace MyTelegram.QueryHandlers.MongoDB.Story;

public class GetStoryAlbumStoriesQueryHandler(
    IQueryOnlyReadModelStore<StoryAlbumReadModel> albumStore,
    IQueryOnlyReadModelStore<StoryReadModel> storyStore)
    : IQueryHandler<GetStoryAlbumStoriesQuery, IReadOnlyCollection<IStoryReadModel>>
{
    public async Task<IReadOnlyCollection<IStoryReadModel>> ExecuteQueryAsync(GetStoryAlbumStoriesQuery query, CancellationToken cancellationToken)
    {
        var album = await albumStore.FirstOrDefaultAsync(
            p => p.OwnerPeerId == query.OwnerPeerId && p.AlbumId == query.AlbumId, cancellationToken);
        if (album == null || album.StoryIds.Count == 0)
            return [];

        var storyIds = album.StoryIds;
        return await storyStore.FindAsync(
            p => p.OwnerPeerId == query.OwnerPeerId && storyIds.Contains(p.StoryId), cancellationToken: cancellationToken);
    }
}
