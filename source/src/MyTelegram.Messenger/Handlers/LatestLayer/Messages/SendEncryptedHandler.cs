namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Sends a text message to a secret chat.
/// Possible errors
/// Code Type Description
/// 400 CHAT_ID_INVALID The provided chat id is invalid.
/// 400 DATA_INVALID Encrypted data invalid.
/// 400 DATA_TOO_LONG Data too long.
/// 400 ENCRYPTION_DECLINED The secret chat was declined.
/// 500 MSG_WAIT_FAILED A waiting call returned an error.
/// 403 USER_IS_BLOCKED You were blocked by this user.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.sendEncrypted"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class SendEncryptedHandler(ICommandBus commandBus, IIdGenerator idGenerator, IQueryProcessor queryProcessor, IObjectMessageSender messageSender)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestSendEncrypted, MyTelegram.Schema.Messages.ISentEncryptedMessage>
{
    protected override async Task<MyTelegram.Schema.Messages.ISentEncryptedMessage> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestSendEncrypted obj)
    {
        var peer = obj.Peer as TInputEncryptedChat;
        if (peer is null)
            RpcErrors.RpcErrors400.ChatIdInvalid.ThrowRpcError();

        var chat = await queryProcessor.ProcessAsync(new GetEncryptedChatByIdQuery(peer!.ChatId), default);
        if (chat == null)
            RpcErrors.RpcErrors400.ChatIdInvalid.ThrowRpcError();
        if (chat!.ChatState == "discarded")
            RpcErrors.RpcErrors400.EncryptionDeclined.ThrowRpcError();

        var isAdmin = input.UserId == chat.AdminId;
        var recipientId = isAdmin ? chat.ParticipantId : chat.AdminId;
        var recipientPermAuthKeyId = isAdmin ? chat.ParticipantPermAuthKeyId : chat.AdminPermAuthKeyId;

        var qts = await idGenerator.NextIdAsync(IdType.Qts, recipientId);
        var date = CurrentDate;

        var command = new SendEncryptedMessageCommand(
            EncryptedMessageId.Create(obj.RandomId),
            peer!.ChatId, recipientId, recipientPermAuthKeyId,
            obj.Data.ToArray(), null, qts, obj.RandomId, SendMessageType.Text, date);
        await commandBus.PublishAsync(command, default);


        return new MyTelegram.Schema.Messages.TSentEncryptedMessage { Date = date };
    }
}