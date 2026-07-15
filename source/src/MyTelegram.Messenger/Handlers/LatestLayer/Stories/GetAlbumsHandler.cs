using MyTelegram.Schema.Stories;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Get story albums created by a peer.
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.getAlbums"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetAlbumsHandler(
    IPeerHelper peerHelper,
    IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestGetAlbums, MyTelegram.Schema.Stories.IAlbums>, IObjectHandler
{
    protected override async Task<MyTelegram.Schema.Stories.IAlbums> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestGetAlbums obj)
    {
        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);
        var albums = await queryProcessor.ProcessAsync(
            new GetStoryAlbumsByPeerQuery(ownerPeer.PeerId), default);

        return new TAlbums
        {
            Hash = 0,
            Albums = new TVector<MyTelegram.Schema.IStoryAlbum>(
                albums.Select(a => (MyTelegram.Schema.IStoryAlbum)new TStoryAlbum
                {
                    AlbumId = a.AlbumId,
                    Title = a.Title
                }))
        };
    }
}
