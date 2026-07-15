namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Pin or unpin one or more stories
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.togglePinned"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class TogglePinnedHandler(
    ICommandBus commandBus,
    IQueryProcessor queryProcessor,
    IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestTogglePinned, TVector<int>>
{
    protected override async Task<TVector<int>> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestTogglePinned obj)
    {
        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);
        var toggled = new List<int>();

        foreach (var storyId in obj.Id)
        {
            var existing = await queryProcessor.ProcessAsync(
                new GetStoryByIdQuery(ownerPeer.PeerId, storyId), default);
            if (existing == null)
                continue;

            var command = new TogglePinnedCommand(
                StoryId.Create(ownerPeer.PeerId, storyId),
                obj.Pinned);
            await commandBus.PublishAsync(command, default);
            toggled.Add(storyId);
        }

        return new TVector<int>(toggled);
    }
}
