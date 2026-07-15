namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Obtain full info about a set of <a href="https://corefork.telegram.org/api/stories">stories</a> by their IDs.
/// Possible errors
/// Code Type Description
/// 400 STORY_ID_EMPTY You specified no story IDs.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.getStoriesByID"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetStoriesByIDHandler(
    IQueryProcessor queryProcessor,
    IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestGetStoriesByID, MyTelegram.Schema.Stories.IStories>
{
    protected override async Task<MyTelegram.Schema.Stories.IStories> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestGetStoriesByID obj)
    {
        if (obj.Id == null || obj.Id.Count == 0)
            RpcErrors.RpcErrors400.StoryIdEmpty.ThrowRpcError();

        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);
        var isOwner = ownerPeer.PeerId == input.UserId;

        var stories = await queryProcessor.ProcessAsync(
            new GetStoriesByIdListQuery(ownerPeer.PeerId, obj.Id.ToList()), default);

        var schemaStories = stories
            .OrderBy(s => s.StoryId)
            .Select(s => (IStoryItem)StoryBuilderHelper.BuildFromReadModel(s, isOwner))
            .ToList();

        return new MyTelegram.Schema.Stories.TStories
        {
            Count = schemaStories.Count,
            Stories = new TVector<IStoryItem>(schemaStories),
            Chats = [],
            Users = []
        };
    }
}
