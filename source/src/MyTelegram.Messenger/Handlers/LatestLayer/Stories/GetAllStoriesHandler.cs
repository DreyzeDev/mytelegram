using MyTelegram.Schema.Stories;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Fetch the List of active (or active and hidden) stories, see <a href="https://corefork.telegram.org/api/stories#watching-stories">here »</a> for more info on watching stories.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.getAllStories"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetAllStoriesHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestGetAllStories, MyTelegram.Schema.Stories.IAllStories>
{
    protected override async Task<MyTelegram.Schema.Stories.IAllStories> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestGetAllStories obj)
    {
        var myStories = await queryProcessor.ProcessAsync(
            new GetActiveStoriesQuery(input.UserId), default);

        var peerStoriesList = new List<MyTelegram.Schema.IPeerStories>();

        if (myStories.Count > 0)
        {
            var schemaStories = myStories
                .OrderByDescending(s => s.StoryId)
                .Select(s => (IStoryItem)StoryBuilderHelper.BuildFromReadModel(s, isOwner: true))
                .ToList();

            peerStoriesList.Add(new MyTelegram.Schema.TPeerStories
            {
                Peer = new TPeerUser { UserId = input.UserId },
                Stories = new TVector<IStoryItem>(schemaStories),
                MaxReadId = null
            });
        }

        return new TAllStories
        {
            HasMore = false,
            Count = peerStoriesList.Count,
            State = string.Empty,
            PeerStories = new TVector<MyTelegram.Schema.IPeerStories>(peerStoriesList),
            Chats = [],
            Users = [],
            StealthMode = new TStoriesStealthMode()
        };
    }
}
