namespace MyTelegram.Messenger.Handlers.LatestLayer.Chatlists;
/// <summary>
/// Delete a folder imported using a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>
/// Possible errors
/// Code Type Description
/// 400 FILTER_ID_INVALID The specified filter ID is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/chatlists.leaveChatlist"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class LeaveChatlistHandler(ICommandBus commandBus)
    : RpcResultObjectHandler<MyTelegram.Schema.Chatlists.RequestLeaveChatlist, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Chatlists.RequestLeaveChatlist obj)
    {
        var command = new DeleteDialogFilterCommand(
            DialogFilterId.Create(input.UserId, obj.Chatlist.FilterId),
            input.ToRequestInfo());
        await commandBus.PublishAsync(command, default);
        return new TUpdates { Updates = [], Chats = [], Users = [], Date = CurrentDate };
    }
}
