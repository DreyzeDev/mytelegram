namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Rename a story album, or add, delete or reorder stories in it.
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.updateAlbum"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class UpdateAlbumHandler(
    ICommandBus commandBus,
    IPeerHelper peerHelper,
    IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestUpdateAlbum, MyTelegram.Schema.IStoryAlbum>, IObjectHandler
{
    protected override async Task<MyTelegram.Schema.IStoryAlbum> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestUpdateAlbum obj)
    {
        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);
        var existing = await queryProcessor.ProcessAsync(
            new GetStoryAlbumByIdQuery(ownerPeer.PeerId, obj.AlbumId), default);
        if (existing == null)
            RpcErrors.RpcErrors400.PeerIdInvalid.ThrowRpcError();

        var command = new UpdateAlbumCommand(
            StoryAlbumId.Create(ownerPeer.PeerId, obj.AlbumId),
            obj.Title,
            obj.AddStories?.ToList(),
            obj.DeleteStories?.ToList(),
            obj.Order?.ToList());
        await commandBus.PublishAsync(command, default);

        var newTitle = obj.Title ?? existing!.Title;
        var storyIds = new List<int>(existing!.StoryIds);
        if (obj.AddStories != null)
            foreach (var id in obj.AddStories.Where(id => !storyIds.Contains(id)))
                storyIds.Add(id);
        if (obj.DeleteStories != null)
            storyIds.RemoveAll(id => obj.DeleteStories.Contains(id));
        if (obj.Order != null)
            storyIds = obj.Order.Where(id => storyIds.Contains(id)).ToList();

        return new TStoryAlbum
        {
            AlbumId = obj.AlbumId,
            Title = newTitle
        };
    }
}
