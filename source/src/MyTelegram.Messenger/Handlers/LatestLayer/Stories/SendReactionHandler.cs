namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// React to a story.
/// Possible errors
/// Code Type Description
/// 400 STORY_ID_INVALID The specified story ID is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.sendReaction"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class SendReactionHandler : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestSendReaction, MyTelegram.Schema.IUpdates>
{
    protected override Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestSendReaction obj)
    {
        return Task.FromResult<IUpdates>(new TUpdates
        {
            Updates = [],
            Users = [],
            Chats = [],
            Date = CurrentDate,
            Seq = 0
        });
    }
}
