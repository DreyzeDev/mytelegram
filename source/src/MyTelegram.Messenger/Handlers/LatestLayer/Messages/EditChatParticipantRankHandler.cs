namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;

/// <summary>
/// Set the rank of an admin in a supergroup/channel
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.editChatAdmin"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class EditChatParticipantRankHandler(IPeerHelper peerHelper, ICommandBus commandBus, IChannelAppService channelAppService, IQueryProcessor queryProcessor, IChannelAdminRightsChecker channelAdminRightsChecker) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestEditChatParticipantRank, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestEditChatParticipantRank obj)
    {
        var peer = peerHelper.GetPeer(obj.Peer, input.UserId);
        if (peer.PeerType is not (PeerType.Channel or PeerType.Chat))
        {
            RpcErrors.RpcErrors400.PeerIdInvalid.ThrowRpcError();
        }

        var channelReadModel = await channelAppService.GetAsync(peer.PeerId);
        channelReadModel.ThrowExceptionIfChannelDeleted();

        await channelAdminRightsChecker.CheckAdminRightAsync(peer.PeerId, input.UserId, adminRights => adminRights.AddAdmins);

        var participant = peerHelper.GetPeer(obj.Participant, input.UserId);
        var admin = channelReadModel!.AdminList.FirstOrDefault(p => p.UserId == participant.PeerId);
        if (admin == null)
        {
            RpcErrors.RpcErrors400.UserNotParticipant.ThrowRpcError();
            return null!;
        }

        if (participant.PeerId == channelReadModel.CreatorId && input.UserId != channelReadModel.CreatorId)
        {
            RpcErrors.RpcErrors400.ChatAdminRequired.ThrowRpcError();
        }

        var channelMember = await queryProcessor.ProcessAsync(new GetChannelMemberByUserIdQuery(channelReadModel.ChannelId, participant.PeerId));
        var isBot = peerHelper.IsBotUser(participant.PeerId);
        var command = new EditChannelAdminCommand(ChannelId.Create(channelReadModel.ChannelId), input.ToRequestInfo(), input.UserId, admin.CanEdit, participant.PeerId, isBot, channelMember != null, admin.AdminRights, obj.Rank, CurrentDate, false);
        await commandBus.PublishAsync(command);
        return null !;
    }
}
