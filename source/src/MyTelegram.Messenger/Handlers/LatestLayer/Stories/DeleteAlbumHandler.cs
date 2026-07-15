namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Delete a story album.
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.deleteAlbum"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class DeleteAlbumHandler(
    ICommandBus commandBus,
    IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestDeleteAlbum, IBool>, IObjectHandler
{
    protected override async Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestDeleteAlbum obj)
    {
        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);
        var command = new DeleteAlbumCommand(
            StoryAlbumId.Create(ownerPeer.PeerId, obj.AlbumId));
        await commandBus.PublishAsync(command, default);
        return new TBoolTrue();
    }
}
