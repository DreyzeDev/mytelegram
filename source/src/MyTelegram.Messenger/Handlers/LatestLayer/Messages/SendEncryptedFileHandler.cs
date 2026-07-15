namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Sends a message with a file attachment to a secret chat
/// Possible errors
/// Code Type Description
/// 400 CHAT_ID_INVALID The provided chat id is invalid.
/// 400 DATA_TOO_LONG Data too long.
/// 400 ENCRYPTION_DECLINED The secret chat was declined.
/// 400 FILE_EMTPY An empty file was provided.
/// 400 MD5_CHECKSUM_INVALID The MD5 checksums do not match.
/// 400 MSG_WAIT_FAILED A waiting call returned an error.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.sendEncryptedFile"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class SendEncryptedFileHandler(ICommandBus commandBus, IIdGenerator idGenerator, IQueryProcessor queryProcessor, IObjectMessageSender messageSender)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestSendEncryptedFile, MyTelegram.Schema.Messages.ISentEncryptedMessage>
{
    protected override async Task<MyTelegram.Schema.Messages.ISentEncryptedMessage> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestSendEncryptedFile obj)
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

        // Resolve the attached encrypted file
        IEncryptedFile encryptedFile = obj.File switch
        {
            TInputEncryptedFileUploaded f => new TEncryptedFile
            {
                Id = f.Id, AccessHash = Random.Shared.NextInt64(),
                Size = 0, DcId = 1, KeyFingerprint = f.KeyFingerprint
            },
            TInputEncryptedFileBigUploaded f => new TEncryptedFile
            {
                Id = f.Id, AccessHash = Random.Shared.NextInt64(),
                Size = 0, DcId = 1, KeyFingerprint = f.KeyFingerprint
            },
            _ => new TEncryptedFileEmpty()
        };

        var fileBytes = encryptedFile.ToBytes();

        var command = new SendEncryptedMessageCommand(
            EncryptedMessageId.Create(obj.RandomId),
            peer!.ChatId, recipientId, recipientPermAuthKeyId,
            obj.Data.ToArray(), fileBytes, qts, obj.RandomId, SendMessageType.Media, date);
        await commandBus.PublishAsync(command, default);


        return new MyTelegram.Schema.Messages.TSentEncryptedFile { Date = date, File = encryptedFile };
    }
}