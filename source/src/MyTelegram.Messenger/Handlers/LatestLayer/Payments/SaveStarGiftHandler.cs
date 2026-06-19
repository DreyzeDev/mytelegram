namespace MyTelegram.Messenger.Handlers.LatestLayer.Payments;

internal sealed class SaveStarGiftHandler(ICommandBus commandBus, IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Payments.RequestSaveStarGift, IBool>
{
    protected override async Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Payments.RequestSaveStarGift obj)
    {
        int msgId;
        long ownerPeerId;

        switch (obj.Stargift)
        {
            case TInputSavedStarGiftUser u:
                msgId = u.MsgId;
                ownerPeerId = input.UserId;
                break;
            case TInputSavedStarGiftChat c:
                msgId = (int)c.SavedId;
                ownerPeerId = c.Peer is TInputPeerChannel ch ? ch.ChannelId :
                              c.Peer is TInputPeerUser pu ? pu.UserId : input.UserId;
                break;
            default:
                RpcErrors.RpcErrors400.MessageIdInvalid.ThrowRpcError();
                return default!;
        }

        var existing = await queryProcessor.ProcessAsync(new GetUserStarGiftByMsgIdQuery(ownerPeerId, msgId));
        if (existing == null)
            RpcErrors.RpcErrors400.StargiftNotFound.ThrowRpcError();

        var command = new SaveGiftCommand(
            UserStarGiftId.Create(ownerPeerId, msgId),
            input.ToRequestInfo(),
            obj.Unsave);

        await commandBus.PublishAsync(command);
        return new TBoolTrue();
    }
}
