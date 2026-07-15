namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;

/// <summary>
/// Transfer a chat
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.editChatCreator"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class EditChatCreatorHandler(IPeerHelper peerHelper, ICommandBus commandBus, IChannelAppService channelAppService, IQueryProcessor queryProcessor) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestEditChatCreator, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestEditChatCreator obj)
    {
        var peer = peerHelper.GetPeer(obj.Peer, input.UserId);
        if (peer.PeerType is not (PeerType.Channel or PeerType.Chat))
        {
            RpcErrors.RpcErrors400.PeerIdInvalid.ThrowRpcError();
        }

        if (obj.Password is not TInputCheckPasswordSRP)
        {
            RpcErrors.RpcErrors400.PasswordMissing.ThrowRpcError();
        }

        var channelReadModel = await channelAppService.GetAsync(peer.PeerId);
        channelReadModel.ThrowExceptionIfChannelDeleted();
        if (channelReadModel!.CreatorId != input.UserId)
        {
            RpcErrors.RpcErrors400.ChatAdminRequired.ThrowRpcError();
        }

        var newCreatorPeer = peerHelper.GetPeer(obj.UserId, input.UserId);
        if (newCreatorPeer.PeerId == channelReadModel.CreatorId)
        {
            RpcErrors.RpcErrors400.UserCreator.ThrowRpcError();
        }

        var newCreatorMember = await queryProcessor.ProcessAsync(new GetChannelMemberByUserIdQuery(channelReadModel.ChannelId, newCreatorPeer.PeerId));
        if (newCreatorMember == null || newCreatorMember.Kicked || newCreatorMember.Left)
        {
            RpcErrors.RpcErrors400.UserNotParticipant.ThrowRpcError();
        }

        var command = new TransferChannelOwnershipCommand(ChannelId.Create(peer.PeerId), input.ToRequestInfo(), newCreatorPeer.PeerId);
        await commandBus.PublishAsync(command);
        return null !;
    }
}
