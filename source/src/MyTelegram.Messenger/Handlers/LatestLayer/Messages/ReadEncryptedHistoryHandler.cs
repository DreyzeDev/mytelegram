namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Marks message history within a secret chat as read.
/// Possible errors
/// Code Type Description
/// 400 CHAT_ID_INVALID The provided chat id is invalid.
/// 400 MAX_DATE_INVALID The specified maximum date is invalid.
/// 400 MSG_WAIT_FAILED A waiting call returned an error.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.readEncryptedHistory"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class ReadEncryptedHistoryHandler(IQueryProcessor queryProcessor, IObjectMessageSender messageSender)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestReadEncryptedHistory, IBool>
{
    protected override async Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestReadEncryptedHistory obj)
    {
        var peer = obj.Peer as TInputEncryptedChat;
        if (peer is null)
            RpcErrors.RpcErrors400.ChatIdInvalid.ThrowRpcError();

        var chat = await queryProcessor.ProcessAsync(new GetEncryptedChatByIdQuery(peer!.ChatId), default);
        if (chat == null)
            RpcErrors.RpcErrors400.ChatIdInvalid.ThrowRpcError();

        var date = CurrentDate;
        var otherUserId = input.UserId == chat!.AdminId ? chat.ParticipantId : chat.AdminId;
        await messageSender.PushMessageToPeerAsync(
            new Peer(PeerType.User, otherUserId),
            new TUpdateShort
            {
                Date = date,
                Update = new TUpdateEncryptedMessagesRead { ChatId = peer!.ChatId, MaxDate = obj.MaxDate, Date = date }
            },
            excludeAuthKeyId: null);

        return new TBoolTrue();
    }
}