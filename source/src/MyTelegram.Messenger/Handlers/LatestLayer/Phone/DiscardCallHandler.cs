namespace MyTelegram.Messenger.Handlers.LatestLayer.Phone;
/// <summary>
/// Refuse or end running call
/// Possible errors
/// Code Type Description
/// 400 CALL_PEER_INVALID The provided call peer object is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/phone.discardCall"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class DiscardCallHandler(
    ICommandBus commandBus,
    IQueryProcessor queryProcessor,
    IObjectMessageSender messageSender)
    : RpcResultObjectHandler<MyTelegram.Schema.Phone.RequestDiscardCall, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Phone.RequestDiscardCall obj)
    {
        var peer = obj.Peer as TInputPhoneCall;
        if (peer is null)
            RpcErrors.RpcErrors400.CallPeerInvalid.ThrowRpcError();

        var call = await queryProcessor.ProcessAsync(new GetPhoneCallByIdQuery(peer!.Id), default);
        if (call == null)
            RpcErrors.RpcErrors400.CallPeerInvalid.ThrowRpcError();

        var reasonType = obj.Reason switch
        {
            TPhoneCallDiscardReasonHangup => "hangup",
            TPhoneCallDiscardReasonBusy => "busy",
            TPhoneCallDiscardReasonDisconnect => "disconnect",
            TPhoneCallDiscardReasonMissed => "missed",
            _ => "hangup"
        };

        var command = new DiscardPhoneCallCommand(
            PhoneCallId.Create(peer!.Id),
            reasonType, obj.Duration, obj.Video);
        await commandBus.PublishAsync(command, default);

        var discarded = new TPhoneCallDiscarded
        {
            Id = call!.CallId,
            Reason = obj.Reason,
            Duration = obj.Duration,
            Video = obj.Video
        };
        var update = new TUpdatePhoneCall { PhoneCall = discarded };
        var date = CurrentDate;

        // Push to the other party
        var otherUserId = input.UserId == call.CallerId ? call.CalleeId : call.CallerId;
        await messageSender.PushMessageToPeerAsync(
            new Peer(PeerType.User, otherUserId),
            new TUpdateShort { Date = date, Update = update },
            excludeAuthKeyId: null);

        return new TUpdates
        {
            Updates = new TVector<IUpdate>(update),
            Users = [],
            Chats = [],
            Date = date,
            Seq = 0
        };
    }
}
