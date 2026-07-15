namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Obtain the list of users that have viewed a specific story we posted.
/// Possible errors
/// Code Type Description
/// 400 STORY_ID_INVALID The specified story ID is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.getStoryViewsList"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetStoryViewsListHandler(
    IQueryProcessor queryProcessor,
    IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestGetStoryViewsList, MyTelegram.Schema.Stories.IStoryViewsList>
{
    protected override async Task<MyTelegram.Schema.Stories.IStoryViewsList> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestGetStoryViewsList obj)
    {
        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);

        var story = await queryProcessor.ProcessAsync(
            new GetStoryByIdQuery(ownerPeer.PeerId, obj.Id), default);
        if (story == null)
            RpcErrors.RpcErrors400.StoryIdInvalid.ThrowRpcError();

        var recentViewers = story!.RecentViewers ?? [];
        var viewItems = recentViewers
            .Select(userId => (IStoryView)new TStoryView
            {
                UserId = userId,
                Date = story.Date,
                Reaction = null
            })
            .ToList();

        return new MyTelegram.Schema.Stories.TStoryViewsList
        {
            Count = viewItems.Count,
            ViewsCount = story.ViewsCount,
            ForwardsCount = 0,
            ReactionsCount = 0,
            Views = new TVector<IStoryView>(viewItems),
            Chats = [],
            Users = [],
            NextOffset = null
        };
    }
}
