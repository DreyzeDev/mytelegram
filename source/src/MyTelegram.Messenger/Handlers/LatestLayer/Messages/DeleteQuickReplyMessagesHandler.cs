namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Delete one or more messages from a <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shortcut</a>. This will also emit an <a href="https://corefork.telegram.org/constructor/updateDeleteQuickReplyMessages">updateDeleteQuickReplyMessages</a> update.
/// Possible errors
/// Code Type Description
/// 400 SHORTCUT_INVALID The specified shortcut is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.deleteQuickReplyMessages"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class DeleteQuickReplyMessagesHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestDeleteQuickReplyMessages, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestDeleteQuickReplyMessages obj)
    {
        var shortcut = await queryProcessor.ProcessAsync(new GetQuickReplyShortcutByIdQuery(input.UserId, obj.ShortcutId));
        if (shortcut == null)
        {
            RpcErrors.RpcErrors400.ShortcutInvalid.ThrowRpcError();
            return default!;
        }

        var deletedIds = new TVector<int>(obj.Id);
        var update = new TUpdateDeleteQuickReplyMessages
        {
            ShortcutId = obj.ShortcutId,
            Messages = deletedIds
        };

        return new TUpdates
        {
            Updates = new TVector<IUpdate> { update },
            Chats = [],
            Users = [],
            Date = CurrentDate
        };
    }
}
