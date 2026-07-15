namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Sends a request to start a secret chat to the user.
/// Possible errors
/// Code Type Description
/// 400 DH_G_A_INVALID g_a invalid.
/// 400 INPUT_USER_DEACTIVATED The specified user was deleted.
/// 400 USER_ID_INVALID The provided user ID is invalid.
/// 403 USER_IS_BLOCKED You were blocked by this user.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.requestEncryption"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class RequestEncryptionHandler(ICommandBus commandBus, IObjectMessageSender messageSender)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestRequestEncryption, MyTelegram.Schema.IEncryptedChat>
{
    protected override async Task<MyTelegram.Schema.IEncryptedChat> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestRequestEncryption obj)
    {
        var inputUser = obj.UserId as TInputUser;
        if (inputUser is null)
            RpcErrors.RpcErrors400.UserIdInvalid.ThrowRpcError();

        var participantId = inputUser!.UserId;
        var adminId = input.UserId;
        var chatId = obj.RandomId;
        var accessHash = Random.Shared.NextInt64();
        var date = CurrentDate;

        var command = new RequestEncryptedChatCommand(
            EncryptedChatId.Create(chatId),
            chatId, accessHash, adminId, participantId,
            obj.GA, input.PermAuthKeyId, date);
        await commandBus.PublishAsync(command, default);


        return new TEncryptedChatWaiting
        {
            Id = chatId,
            AccessHash = accessHash,
            Date = date,
            AdminId = adminId,
            ParticipantId = participantId
        };
    }
}
