namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Increment the view counter of one or more stories.
/// Possible errors
/// Code Type Description
/// 400 STORY_ID_EMPTY You specified no story IDs.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.incrementStoryViews"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class IncrementStoryViewsHandler(
    ICommandBus commandBus,
    IQueryProcessor queryProcessor,
    IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestIncrementStoryViews, IBool>
{
    protected override async Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestIncrementStoryViews obj)
    {
        if (obj.Id == null || obj.Id.Count == 0)
            RpcErrors.RpcErrors400.StoryIdEmpty.ThrowRpcError();

        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);
        var date = CurrentDate;

        foreach (var storyId in obj.Id)
        {
            var existing = await queryProcessor.ProcessAsync(
                new GetStoryByIdQuery(ownerPeer.PeerId, storyId), default);
            if (existing == null)
                continue;

            if (existing.OwnerPeerId == input.UserId)
                continue;

            var command = new IncrementViewCommand(
                StoryId.Create(ownerPeer.PeerId, storyId),
                input.UserId,
                date);
            await commandBus.PublishAsync(command, default);
        }

        return new TBoolTrue();
    }
}
