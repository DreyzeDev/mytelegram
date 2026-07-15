namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Send typing event by the current user to a secret chat.
/// Possible errors
/// Code Type Description
/// 400 CHAT_ID_INVALID The provided chat id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.setEncryptedTyping"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class SetEncryptedTypingHandler(IQueryProcessor queryProcessor, IObjectMessageSender messageSender)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestSetEncryptedTyping, IBool>
{
    protected override async Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestSetEncryptedTyping obj)
    {
        var peer = obj.Peer as TInputEncryptedChat;
        if (peer is null)
            RpcErrors.RpcErrors400.ChatIdInvalid.ThrowRpcError();

        var chat = await queryProcessor.ProcessAsync(new GetEncryptedChatByIdQuery(peer!.ChatId), default);
        if (chat == null)
            RpcErrors.RpcErrors400.ChatIdInvalid.ThrowRpcError();

        if (obj.Typing)
        {
            var otherUserId = input.UserId == chat!.AdminId ? chat.ParticipantId : chat.AdminId;
            await messageSender.PushMessageToPeerAsync(
                new Peer(PeerType.User, otherUserId),
                new TUpdateShort { Date = CurrentDate, Update = new TUpdateEncryptedChatTyping { ChatId = peer!.ChatId } },
                excludeAuthKeyId: null);
        }

        return new TBoolTrue();
    }
}