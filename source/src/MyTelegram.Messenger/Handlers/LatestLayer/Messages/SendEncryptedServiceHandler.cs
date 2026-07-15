namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Sends a service message to a secret chat.
/// Possible errors
/// Code Type Description
/// 400 CHAT_ID_INVALID The provided chat id is invalid.
/// 400 DATA_INVALID Encrypted data invalid.
/// 400 ENCRYPTION_DECLINED The secret chat was declined.
/// 400 ENCRYPTION_ID_INVALID The provided secret chat ID is invalid.
/// 500 MSG_WAIT_FAILED A waiting call returned an error.
/// 403 USER_DELETED You can't send this secret message because the other participant deleted their account.
/// 403 USER_IS_BLOCKED You were blocked by this user.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.sendEncryptedService"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class SendEncryptedServiceHandler(ICommandBus commandBus, IIdGenerator idGenerator, IQueryProcessor queryProcessor, IObjectMessageSender messageSender)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestSendEncryptedService, MyTelegram.Schema.Messages.ISentEncryptedMessage>
{
    protected override async Task<MyTelegram.Schema.Messages.ISentEncryptedMessage> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestSendEncryptedService obj)
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
            obj.Data.ToArray(), null, qts, obj.RandomId, SendMessageType.MessageService, date);
        await commandBus.PublishAsync(command, default);


        return new MyTelegram.Schema.Messages.TSentEncryptedMessage { Date = date };
    }
}