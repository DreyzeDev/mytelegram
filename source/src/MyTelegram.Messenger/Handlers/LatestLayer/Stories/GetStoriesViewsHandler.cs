namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Obtain info about the view count, forward count, reactions and recent viewers of one or more stories.
/// Possible errors
/// Code Type Description
/// 400 STORY_ID_EMPTY You specified no story IDs.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.getStoriesViews"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetStoriesViewsHandler(
    IQueryProcessor queryProcessor,
    IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestGetStoriesViews, MyTelegram.Schema.Stories.IStoryViews>
{
    protected override async Task<MyTelegram.Schema.Stories.IStoryViews> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestGetStoriesViews obj)
    {
        if (obj.Id == null || obj.Id.Count == 0)
            RpcErrors.RpcErrors400.StoryIdEmpty.ThrowRpcError();

        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);

        var stories = await queryProcessor.ProcessAsync(
            new GetStoriesByIdListQuery(ownerPeer.PeerId, obj.Id.ToList()), default);

        var storyMap = stories.ToDictionary(s => s.StoryId);

        var views = obj.Id
            .Select(id =>
            {
                storyMap.TryGetValue(id, out var story);
                var recentViewers = story?.RecentViewers?.Count > 0
                    ? new TVector<long>(story.RecentViewers)
                    : null;

                return (IStoryViews)new TStoryViews
                {
                    ViewsCount = story?.ViewsCount ?? 0,
                    HasViewers = (story?.ViewsCount ?? 0) > 0,
                    RecentViewers = recentViewers
                };
            })
            .ToList();

        return new MyTelegram.Schema.Stories.TStoryViews
        {
            Views = new TVector<IStoryViews>(views),
            Users = []
        };
    }
}
