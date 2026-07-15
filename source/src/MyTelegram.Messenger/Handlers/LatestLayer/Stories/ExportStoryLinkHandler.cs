namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Generate a story deep link for a specific story.
/// Possible errors
/// Code Type Description
/// 400 STORY_ID_INVALID The specified story ID is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.exportStoryLink"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class ExportStoryLinkHandler(
    IQueryProcessor queryProcessor,
    IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestExportStoryLink, MyTelegram.Schema.IExportedStoryLink>
{
    protected override async Task<MyTelegram.Schema.IExportedStoryLink> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestExportStoryLink obj)
    {
        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);

        var story = await queryProcessor.ProcessAsync(
            new GetStoryByIdQuery(ownerPeer.PeerId, obj.Id), default);
        if (story == null)
            RpcErrors.RpcErrors400.StoryIdInvalid.ThrowRpcError();

        var link = $"https://t.me/c/{ownerPeer.PeerId}/{obj.Id}";
        return new TExportedStoryLink { Link = link };
    }
}
