using MyTelegram.Schema.Stories;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Fetch the <a href="https://corefork.telegram.org/api/stories#pinned-or-archived-stories">stories</a> pinned on a peer's profile.
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.getPinnedStories"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetPinnedStoriesHandler(
    IQueryProcessor queryProcessor,
    IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestGetPinnedStories, MyTelegram.Schema.Stories.IStories>
{
    protected override async Task<MyTelegram.Schema.Stories.IStories> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestGetPinnedStories obj)
    {
        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);
        var isOwner = ownerPeer.PeerId == input.UserId;
        var limit = obj.Limit > 0 ? obj.Limit : 100;

        var stories = await queryProcessor.ProcessAsync(
            new GetPinnedStoriesQuery(ownerPeer.PeerId, obj.OffsetId, limit), default);

        var schemaStories = stories
            .Select(s => (IStoryItem)StoryBuilderHelper.BuildFromReadModel(s, isOwner))
            .ToList();

        return new TStories
        {
            Count = schemaStories.Count,
            Stories = new TVector<IStoryItem>(schemaStories),
            Chats = [],
            Users = []
        };
    }
}
