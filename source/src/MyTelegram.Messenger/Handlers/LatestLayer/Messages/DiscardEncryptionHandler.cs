namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Cancels a request for creation and/or delete info on secret chat.
/// Possible errors
/// Code Type Description
/// 400 CHAT_ID_EMPTY The provided chat ID is empty.
/// 400 ENCRYPTION_ALREADY_ACCEPTED Secret chat already accepted.
/// 400 ENCRYPTION_ALREADY_DECLINED The secret chat was already declined.
/// 500 ENCRYPTION_DECLINE_ADMIN_FAILED  
/// 400 ENCRYPTION_ID_INVALID The provided secret chat ID is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.discardEncryption"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class DiscardEncryptionHandler(ICommandBus commandBus, IQueryProcessor queryProcessor, IObjectMessageSender messageSender)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestDiscardEncryption, IBool>
{
    protected override async Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestDiscardEncryption obj)
    {
        var chat = await queryProcessor.ProcessAsync(new GetEncryptedChatByIdQuery(obj.ChatId), default);
        if (chat == null)
            RpcErrors.RpcErrors400.ChatIdInvalid.ThrowRpcError();

        var command = new DiscardEncryptedChatCommand(
            EncryptedChatId.Create(obj.ChatId),
            obj.DeleteHistory);
        await commandBus.PublishAsync(command, default);


        return new TBoolTrue();
    }
}