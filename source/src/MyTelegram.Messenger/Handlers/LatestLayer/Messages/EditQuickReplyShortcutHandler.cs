namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Rename a <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shortcut</a>.<br/>
/// This will emit an <a href="https://corefork.telegram.org/constructor/updateQuickReplies">updateQuickReplies</a> update to other logged-in sessions.
/// Possible errors
/// Code Type Description
/// 403 PREMIUM_ACCOUNT_REQUIRED A premium account is required to execute this action.
/// 400 SHORTCUT_INVALID The specified shortcut is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.editQuickReplyShortcut"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class EditQuickReplyShortcutHandler(ICommandBus commandBus, IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestEditQuickReplyShortcut, IBool>
{
    protected override async Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestEditQuickReplyShortcut obj)
    {
        var shortcut = await queryProcessor.ProcessAsync(new GetQuickReplyShortcutByIdQuery(input.UserId, obj.ShortcutId));
        if (shortcut == null)
        {
            RpcErrors.RpcErrors400.ShortcutInvalid.ThrowRpcError();
            return default!;
        }

        var command = new EditShortcutCommand(
            QuickReplyShortcutId.Create(input.UserId, obj.ShortcutId),
            obj.Shortcut);
        await commandBus.PublishAsync(command, default);
        return new TBoolTrue();
    }
}
