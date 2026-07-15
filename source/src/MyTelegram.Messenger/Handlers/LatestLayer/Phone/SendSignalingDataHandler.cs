namespace MyTelegram.Messenger.Handlers.LatestLayer.Phone;
/// <summary>
/// Send VoIP signaling data to the other call participant.
/// Possible errors
/// Code Type Description
/// 400 CALL_PEER_INVALID The provided call peer object is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/phone.sendSignalingData"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class SendSignalingDataHandler(
    IQueryProcessor queryProcessor,
    IObjectMessageSender messageSender)
    : RpcResultObjectHandler<MyTelegram.Schema.Phone.RequestSendSignalingData, IBool>
{
    protected override async Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Phone.RequestSendSignalingData obj)
    {
        var peer = obj.Peer as TInputPhoneCall;
        if (peer is null)
            RpcErrors.RpcErrors400.CallPeerInvalid.ThrowRpcError();

        var call = await queryProcessor.ProcessAsync(new GetPhoneCallByIdQuery(peer!.Id), default);
        if (call == null)
            RpcErrors.RpcErrors400.CallPeerInvalid.ThrowRpcError();

        var otherUserId = input.UserId == call!.CallerId ? call.CalleeId : call.CallerId;
        var update = new TUpdatePhoneCallSignalingData
        {
            PhoneCallId = call.CallId,
            Data = obj.Data
        };
        await messageSender.PushMessageToPeerAsync(
            new Peer(PeerType.User, otherUserId),
            new TUpdateShort { Date = CurrentDate, Update = update },
            excludeAuthKeyId: null);

        return new TBoolTrue();
    }
}
