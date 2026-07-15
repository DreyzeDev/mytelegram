namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Deletes a user from a chat and sends a service message on it.
/// Possible errors
/// Code Type Description
/// 400 CHAT_ADMIN_REQUIRED You must be an admin in this chat to do this.
/// 400 CHAT_ID_INVALID The provided chat id is invalid.
/// 400 INPUT_USER_DEACTIVATED The specified user was deleted.
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// 400 USER_ID_INVALID The provided user ID is invalid.
/// 400 USER_NOT_PARTICIPANT You're not a member of this supergroup/channel.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.deleteChatUser"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class DeleteChatUserHandler(IPeerHelper peerHelper, ICommandBus commandBus, IChannelAdminRightsChecker channelAdminRightsChecker, IQueryProcessor queryProcessor) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestDeleteChatUser, MyTelegram.Schema.IUpdates>
{
    protected override async Task<IUpdates> HandleCoreAsync(IRequestInput input, RequestDeleteChatUser obj)
    {
        var channelId = obj.ChatId;
        var peer = peerHelper.GetPeer(obj.UserId, input.UserId);

        if (peer.PeerId != input.UserId)
        {
            await channelAdminRightsChecker.CheckAdminRightAsync(channelId, input.UserId, p => p.BanUsers, RpcErrors.RpcErrors400.ChatAdminRequired);
        }

        var bannedRights = ChatBannedRights.CreateDefaultBannedRights();
        bannedRights.ViewMessages = true;
        var command = new EditBannedCommand(ChannelMemberId.Create(channelId, peer.PeerId), input.ToRequestInfo(), input.UserId, channelId, peer.PeerId, bannedRights);
        await commandBus.PublishAsync(command);

        if (obj.RevokeHistory)
        {
            var messageIds = (await queryProcessor.ProcessAsync(new GetMessageIdListByUserIdQuery(channelId, peer.PeerId, MyTelegramConsts.ClearHistoryDefaultPageSize))).ToList();
            if (messageIds.Count > 0)
            {
                var newTopMessageId = await queryProcessor.ProcessAsync(new GetTopMessageIdQuery(channelId, channelId, messageIds));
                var deleteHistoryCommand = new StartDeleteParticipantHistoryCommand(TempId.New, input.ToRequestInfo(), channelId, messageIds, newTopMessageId);
                await commandBus.PublishAsync(deleteHistoryCommand);
            }
        }

        return null !;
    }
}
