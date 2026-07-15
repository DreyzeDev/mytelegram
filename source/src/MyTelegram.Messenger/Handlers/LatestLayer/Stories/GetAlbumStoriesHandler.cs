using MyTelegram.Schema.Stories;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Get stories in a story album.
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.getAlbumStories"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetAlbumStoriesHandler(
    IPeerHelper peerHelper,
    IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestGetAlbumStories, MyTelegram.Schema.Stories.IStories>, IObjectHandler
{
    protected override async Task<MyTelegram.Schema.Stories.IStories> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestGetAlbumStories obj)
    {
        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);
        var allStories = await queryProcessor.ProcessAsync(
            new GetStoryAlbumStoriesQuery(ownerPeer.PeerId, obj.AlbumId), default);

        var limit = obj.Limit > 0 ? obj.Limit : 100;
        var page = allStories
            .Skip(obj.Offset)
            .Take(limit)
            .ToList();

        var isOwner = ownerPeer.PeerId == input.UserId;
        return new TStories
        {
            Stories = new TVector<IStoryItem>(
                page.Select(s => StoryBuilderHelper.BuildFromReadModel(s, isOwner))),
            Chats = [],
            Users = [],
            Count = allStories.Count
        };
    }
}
