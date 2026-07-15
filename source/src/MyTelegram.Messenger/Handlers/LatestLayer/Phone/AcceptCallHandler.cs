namespace MyTelegram.Messenger.Handlers.LatestLayer.Phone;
/// <summary>
/// Accept incoming call
/// Possible errors
/// Code Type Description
/// 400 CALL_ALREADY_ACCEPTED The call was already accepted.
/// 400 CALL_PEER_INVALID The provided call peer object is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/phone.acceptCall"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class AcceptCallHandler(
    ICommandBus commandBus,
    IQueryProcessor queryProcessor,
    IObjectMessageSender messageSender)
    : RpcResultObjectHandler<MyTelegram.Schema.Phone.RequestAcceptCall, MyTelegram.Schema.Phone.IPhoneCall>
{
    protected override async Task<MyTelegram.Schema.Phone.IPhoneCall> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Phone.RequestAcceptCall obj)
    {
        var peer = obj.Peer as TInputPhoneCall;
        if (peer is null)
            RpcErrors.RpcErrors400.CallPeerInvalid.ThrowRpcError();

        var call = await queryProcessor.ProcessAsync(new GetPhoneCallByIdQuery(peer!.Id), default);
        if (call == null)
            RpcErrors.RpcErrors400.CallPeerInvalid.ThrowRpcError();

        if (call!.CallState == "accepted" || call.CallState == "confirmed")
            RpcErrors.RpcErrors400.CallAlreadyAccepted.ThrowRpcError();

        var protocol = PhoneCallBuilderHelper.FromSchema(obj.Protocol);
        var date = CurrentDate;

        var command = new AcceptPhoneCallCommand(
            PhoneCallId.Create(peer!.Id),
            obj.GB, protocol, date);
        await commandBus.PublishAsync(command, default);

        var schemaProtocol = PhoneCallBuilderHelper.ToSchema(protocol);
        var callAccepted = new TPhoneCallAccepted
        {
            Id = call.CallId,
            AccessHash = call.AccessHash,
            Date = call.Date,
            AdminId = call.CallerId,
            ParticipantId = call.CalleeId,
            GB = obj.GB,
            Protocol = schemaProtocol,
            Video = call.IsVideo
        };

        // Push to caller
        await messageSender.PushMessageToPeerAsync(
            new Peer(PeerType.User, call.CallerId),
            new TUpdateShort { Date = date, Update = new TUpdatePhoneCall { PhoneCall = callAccepted } },
            excludeAuthKeyId: null);

        return new MyTelegram.Schema.Phone.TPhoneCall
        {
            PhoneCall = callAccepted,
            Users = []
        };
    }
}
