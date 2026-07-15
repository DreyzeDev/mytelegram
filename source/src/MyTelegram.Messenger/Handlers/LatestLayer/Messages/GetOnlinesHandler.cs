namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Get count of online users in a chat
/// Possible errors
/// Code Type Description
/// 400 CHANNEL_PRIVATE You haven't joined this channel/supergroup.
/// 400 CHAT_ID_INVALID The provided chat id is invalid.
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.getOnlines"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetOnlinesHandler(IPeerHelper peerHelper, IQueryProcessor queryProcessor, IAccessHashHelper accessHashHelper, IChannelAppService channelAppService) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestGetOnlines, MyTelegram.Schema.IChatOnlines>
{
    protected override async Task<MyTelegram.Schema.IChatOnlines> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestGetOnlines obj)
    {
        var peer = peerHelper.GetPeer(obj.Peer, input.UserId);
        if (peer.PeerType != PeerType.Channel)
        {
            // There is no chat-participant read model for basic groups (TInputPeerChat) in this codebase,
            // so only channel/supergroup peers are supported for real online counting.
            RpcErrors.RpcErrors400.PeerIdInvalid.ThrowRpcError();
        }

        await accessHashHelper.CheckAccessHashAsync(input, obj.Peer);
        var channelReadModel = await channelAppService.GetAsync(peer.PeerId);
        channelReadModel.ThrowExceptionIfChannelDeleted();
        if (await channelAppService.SendRpcErrorIfNotChannelMemberAsync(input, channelReadModel!))
        {
            return null!;
        }

        var channelMemberReadModels = await queryProcessor.ProcessAsync(new GetChannelMembersByChannelIdQuery(peer.PeerId, [], 0, int.MaxValue));
        if (channelMemberReadModels.Count == 0)
        {
            return new TChatOnlines { Onlines = 0 };
        }

        var userIds = channelMemberReadModels.Select(p => p.UserId).ToList();
        var users = await queryProcessor.ProcessAsync(new GetUsersByUserIdListQuery(userIds));
        var onlineCount = users.Count(p => p.IsOnline);
        return new TChatOnlines { Onlines = onlineCount };
    }
}