namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Fetch the full active <a href="https://corefork.telegram.org/api/stories#watching-stories">story list</a> of a specific peer.
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.getPeerStories"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetPeerStoriesHandler(
    IQueryProcessor queryProcessor,
    IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestGetPeerStories, MyTelegram.Schema.Stories.IPeerStories>
{
    protected override async Task<MyTelegram.Schema.Stories.IPeerStories> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestGetPeerStories obj)
    {
        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);
        var isOwner = ownerPeer.PeerId == input.UserId;

        var stories = await queryProcessor.ProcessAsync(
            new GetActiveStoriesQuery(ownerPeer.PeerId), default);

        var schemaStories = stories
            .OrderByDescending(s => s.StoryId)
            .Select(s => (IStoryItem)StoryBuilderHelper.BuildFromReadModel(s, isOwner))
            .ToList();

        IPeer peer = ownerPeer.PeerType == PeerType.Channel
            ? new TPeerChannel { ChannelId = ownerPeer.PeerId }
            : new TPeerUser { UserId = ownerPeer.PeerId };

        return new MyTelegram.Schema.Stories.TPeerStories
        {
            Stories = new TPeerStories
            {
                Peer = peer,
                Stories = new TVector<IStoryItem>(schemaStories),
                MaxReadId = null
            },
            Chats = [],
            Users = []
        };
    }
}
