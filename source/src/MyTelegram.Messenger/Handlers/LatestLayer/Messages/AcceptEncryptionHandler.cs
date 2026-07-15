namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Confirms creation of a secret chat
/// Possible errors
/// Code Type Description
/// 400 CHAT_ID_INVALID The provided chat id is invalid.
/// 400 ENCRYPTION_ALREADY_ACCEPTED Secret chat already accepted.
/// 400 ENCRYPTION_ALREADY_DECLINED The secret chat was already declined.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.acceptEncryption"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class AcceptEncryptionHandler(ICommandBus commandBus, IQueryProcessor queryProcessor, IObjectMessageSender messageSender)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestAcceptEncryption, MyTelegram.Schema.IEncryptedChat>
{
    protected override async Task<MyTelegram.Schema.IEncryptedChat> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestAcceptEncryption obj)
    {
        var peer = obj.Peer as TInputEncryptedChat;
        if (peer is null)
            RpcErrors.RpcErrors400.ChatIdInvalid.ThrowRpcError();

        var chat = await queryProcessor.ProcessAsync(new GetEncryptedChatByIdQuery(peer!.ChatId), default);
        if (chat == null)
            RpcErrors.RpcErrors400.ChatIdInvalid.ThrowRpcError();

        if (chat!.ChatState == "accepted")
            RpcErrors.RpcErrors400.EncryptionAlreadyAccepted.ThrowRpcError();
        if (chat.ChatState == "discarded")
            RpcErrors.RpcErrors400.EncryptionAlreadyDeclined.ThrowRpcError();

        var date = CurrentDate;
        var command = new AcceptEncryptedChatCommand(
            EncryptedChatId.Create(peer!.ChatId),
            obj.GB, obj.KeyFingerprint, input.PermAuthKeyId);
        await commandBus.PublishAsync(command, default);


        // Return encryptedChat with GA to participant (Bob) so he can also verify
        return new TEncryptedChat
        {
            Id = (int)chat.ChatId,
            AccessHash = chat.AccessHash,
            Date = date,
            AdminId = chat.AdminId,
            ParticipantId = chat.ParticipantId,
            GAOrB = chat.Ga,
            KeyFingerprint = obj.KeyFingerprint
        };
    }
}
