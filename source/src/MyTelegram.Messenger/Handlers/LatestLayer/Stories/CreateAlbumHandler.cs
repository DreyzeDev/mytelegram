namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Creates a story album.
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.createAlbum"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class CreateAlbumHandler(
    ICommandBus commandBus,
    IIdGenerator idGenerator,
    IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestCreateAlbum, MyTelegram.Schema.IStoryAlbum>, IObjectHandler
{
    protected override async Task<MyTelegram.Schema.IStoryAlbum> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestCreateAlbum obj)
    {
        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);
        var albumId = (int)await idGenerator.NextIdAsync(IdType.StoryAlbumId, ownerPeer.PeerId);
        var storyIds = obj.Stories?.ToList() ?? [];

        var command = new CreateAlbumCommand(
            StoryAlbumId.Create(ownerPeer.PeerId, albumId),
            ownerPeer.PeerId,
            albumId,
            obj.Title,
            storyIds);
        await commandBus.PublishAsync(command, default);

        return new TStoryAlbum
        {
            AlbumId = albumId,
            Title = obj.Title
        };
    }
}
