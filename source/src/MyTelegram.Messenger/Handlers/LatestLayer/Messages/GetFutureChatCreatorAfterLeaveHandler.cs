namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;

/// <summary>
/// Get a suggested successor to take over ownership of a chat when the current creator leaves
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.getFutureChatCreatorAfterLeave"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetFutureChatCreatorAfterLeaveHandler(IPeerHelper peerHelper, IChannelAppService channelAppService, IQueryProcessor queryProcessor, IUserConverterService userConverterService) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestGetFutureChatCreatorAfterLeave, MyTelegram.Schema.IUser>
{
    protected override async Task<MyTelegram.Schema.IUser> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestGetFutureChatCreatorAfterLeave obj)
    {
        var peer = peerHelper.GetPeer(obj.Peer, input.UserId);
        if (peer.PeerType is not (PeerType.Channel or PeerType.Chat))
        {
            RpcErrors.RpcErrors400.PeerIdInvalid.ThrowRpcError();
        }

        var channelReadModel = await channelAppService.GetAsync(peer.PeerId);
        channelReadModel.ThrowExceptionIfChannelDeleted();
        if (channelReadModel!.CreatorId != input.UserId)
        {
            RpcErrors.RpcErrors400.ChatAdminRequired.ThrowRpcError();
        }

        var adminMembers = await queryProcessor.ProcessAsync(new GetChannelMembersByChannelIdQuery(channelReadModel.ChannelId, [], 0, MyTelegramConsts.ChannelAdminMaxCount, OnlyAdmin: true));
        var candidate = adminMembers
            .Where(p => p.UserId != channelReadModel.CreatorId && !p.IsBot && !p.Kicked && !p.Left)
            .OrderBy(p => p.Date)
            .FirstOrDefault();

        if (candidate == null)
        {
            RpcErrors.RpcErrors400.UserIdInvalid.ThrowRpcError();
        }

        var user = await userConverterService.GetUserAsync(input, candidate!.UserId, false, false, input.Layer);
        return user;
    }
}
