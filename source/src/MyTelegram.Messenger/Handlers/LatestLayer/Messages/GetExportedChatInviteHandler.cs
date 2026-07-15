namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Get info about a chat invite
/// Possible errors
/// Code Type Description
/// 400 CHANNEL_INVALID The provided channel is invalid.
/// 400 CHANNEL_PRIVATE You haven't joined this channel/supergroup.
/// 400 CHAT_ADMIN_REQUIRED You must be an admin in this chat to do this.
/// 403 CHAT_WRITE_FORBIDDEN You can't write in this chat.
/// 400 INVITE_HASH_EXPIRED The invite link has expired.
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.getExportedChatInvite"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetExportedChatInviteHandler(IPeerHelper peerHelper, IQueryProcessor queryProcessor, IAccessHashHelper accessHashHelper, IChannelAppService channelAppService, IChatInviteLinkHelper chatInviteLinkHelper, IUserConverterService userConverterService, IChatInviteExportedConverterService chatInviteExportedConverterService) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestGetExportedChatInvite, MyTelegram.Schema.Messages.IExportedChatInvite>
{
    protected override async Task<MyTelegram.Schema.Messages.IExportedChatInvite> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestGetExportedChatInvite obj)
    {
        var peer = peerHelper.GetPeer(obj.Peer, input.UserId);
        if (peer.PeerType is not (PeerType.Channel or PeerType.Chat))
        {
            RpcErrors.RpcErrors400.PeerIdInvalid.ThrowRpcError();
        }

        await accessHashHelper.CheckAccessHashAsync(input, obj.Peer);
        var channelReadModel = await channelAppService.GetAsync(peer.PeerId);
        channelReadModel.ThrowExceptionIfChannelDeleted();
        if (channelReadModel!.AdminList.All(p => p.UserId != input.UserId))
        {
            RpcErrors.RpcErrors400.ChatAdminRequired.ThrowRpcError();
        }

        var link = chatInviteLinkHelper.GetHashFromLink(obj.Link);
        var chatInviteReadModel = await queryProcessor.ProcessAsync(new GetChatInviteQuery(peer.PeerId, link));
        if (chatInviteReadModel == null)
        {
            RpcErrors.RpcErrors400.PeerIdInvalid.ThrowRpcError();
        }

        var invite = chatInviteExportedConverterService.ToExportedChatInvite(chatInviteReadModel!, input.Layer);
        var users = await userConverterService.GetUserListAsync(input, [chatInviteReadModel!.AdminId], false, false, input.Layer);
        return new TExportedChatInvite
        {
            Invite = invite,
            Users = [..users]
        };
    }
}
