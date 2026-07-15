namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Deletes some posted <a href="https://corefork.telegram.org/api/stories">stories</a>.
/// Possible errors
/// Code Type Description
/// 403 BOT_ACCESS_FORBIDDEN
/// 400 STORY_ID_EMPTY You specified no story IDs.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.deleteStories"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class DeleteStoriesHandler(
    ICommandBus commandBus,
    IQueryProcessor queryProcessor,
    IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestDeleteStories, TVector<int>>
{
    protected override async Task<TVector<int>> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestDeleteStories obj)
    {
        if (obj.Id == null || obj.Id.Count == 0)
            RpcErrors.RpcErrors400.StoryIdEmpty.ThrowRpcError();

        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);
        var deleted = new List<int>();

        foreach (var storyId in obj.Id)
        {
            var existing = await queryProcessor.ProcessAsync(
                new GetStoryByIdQuery(ownerPeer.PeerId, storyId), default);
            if (existing == null)
                continue;

            var command = new DeleteStoryCommand(
                StoryId.Create(ownerPeer.PeerId, storyId));
            await commandBus.PublishAsync(command, default);
            deleted.Add(storyId);
        }

        return new TVector<int>(deleted);
    }
}
