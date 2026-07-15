namespace MyTelegram.Messenger.Handlers.LatestLayer.Phone;
/// <summary>
/// Complete phone call E2E encryption key exchange.
/// Possible errors
/// Code Type Description
/// 400 CALL_ALREADY_DECLINED The call was already declined.
/// 400 CALL_PEER_INVALID The provided call peer object is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/phone.confirmCall"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class ConfirmCallHandler(
    ICommandBus commandBus,
    IQueryProcessor queryProcessor,
    IObjectMessageSender messageSender)
    : RpcResultObjectHandler<MyTelegram.Schema.Phone.RequestConfirmCall, MyTelegram.Schema.Phone.IPhoneCall>
{
    protected override async Task<MyTelegram.Schema.Phone.IPhoneCall> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Phone.RequestConfirmCall obj)
    {
        var peer = obj.Peer as TInputPhoneCall;
        if (peer is null)
            RpcErrors.RpcErrors400.CallPeerInvalid.ThrowRpcError();

        var call = await queryProcessor.ProcessAsync(new GetPhoneCallByIdQuery(peer!.Id), default);
        if (call == null)
            RpcErrors.RpcErrors400.CallPeerInvalid.ThrowRpcError();

        var protocol = PhoneCallBuilderHelper.FromSchema(obj.Protocol);
        var date = CurrentDate;

        var command = new ConfirmPhoneCallCommand(
            PhoneCallId.Create(peer!.Id),
            obj.GA, obj.KeyFingerprint, protocol, date);
        await commandBus.PublishAsync(command, default);

        var schemaProtocol = PhoneCallBuilderHelper.ToSchema(protocol);
        var activeCall = new TPhoneCall
        {
            Id = call!.CallId,
            AccessHash = call.AccessHash,
            Date = call.Date,
            AdminId = call.CallerId,
            ParticipantId = call.CalleeId,
            GAOrB = obj.GA,
            KeyFingerprint = obj.KeyFingerprint,
            Protocol = schemaProtocol,
            Connections = [],
            StartDate = date,
            P2pAllowed = true,
            Video = call.IsVideo
        };

        // Push active call to callee
        await messageSender.PushMessageToPeerAsync(
            new Peer(PeerType.User, call.CalleeId),
            new TUpdateShort { Date = date, Update = new TUpdatePhoneCall { PhoneCall = activeCall } },
            excludeAuthKeyId: null);

        return new MyTelegram.Schema.Phone.TPhoneCall
        {
            PhoneCall = activeCall,
            Users = []
        };
    }
}
