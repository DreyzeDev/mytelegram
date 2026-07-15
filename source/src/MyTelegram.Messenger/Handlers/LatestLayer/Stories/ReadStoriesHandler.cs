namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Mark all stories up to a certain ID as read, for a given peer; will emit an updateReadStories update to all logged-in sessions.
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.readStories"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class ReadStoriesHandler(
    IQueryProcessor queryProcessor,
    IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestReadStories, TVector<int>>
{
    protected override async Task<TVector<int>> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestReadStories obj)
    {
        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);

        var stories = await queryProcessor.ProcessAsync(
            new GetActiveStoriesQuery(ownerPeer.PeerId), default);

        var readIds = stories
            .Where(s => s.StoryId <= obj.MaxId)
            .Select(s => s.StoryId)
            .ToList();

        return new TVector<int>(readIds);
    }
}
