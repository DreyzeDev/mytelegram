namespace MyTelegram.Messenger.Handlers.LatestLayer.Phone;
/// <summary>
/// Start a telegram phone call
/// Possible errors
/// Code Type Description
/// 400 CALL_PROTOCOL_FLAGS_INVALID Call protocol flags invalid.
/// 400 USER_ID_INVALID The provided user ID is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/phone.requestCall"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class RequestCallHandler(
    ICommandBus commandBus,
    IIdGenerator idGenerator,
    IObjectMessageSender messageSender)
    : RpcResultObjectHandler<MyTelegram.Schema.Phone.RequestRequestCall, MyTelegram.Schema.Phone.IPhoneCall>
{
    protected override async Task<MyTelegram.Schema.Phone.IPhoneCall> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Phone.RequestRequestCall obj)
    {
        var inputUser = obj.UserId as TInputUser;
        if (inputUser is null)
            RpcErrors.RpcErrors400.UserIdInvalid.ThrowRpcError();

        var calleeId = inputUser!.UserId;
        var callerId = input.UserId;
        var callId = await idGenerator.NextLongIdAsync(IdType.PhoneCallId);
        var accessHash = Random.Shared.NextInt64();
        var date = CurrentDate;
        var protocol = PhoneCallBuilderHelper.FromSchema(obj.Protocol);

        var command = new CreatePhoneCallCommand(
            PhoneCallId.Create(callId),
            callId, accessHash, callerId, calleeId,
            obj.GAHash, protocol, obj.Video, date);
        await commandBus.PublishAsync(command, default);

        var schemaProtocol = PhoneCallBuilderHelper.ToSchema(protocol);

        // Push TPhoneCallRequested to callee
        var callRequested = new TPhoneCallRequested
        {
            Id = callId,
            AccessHash = accessHash,
            Date = date,
            AdminId = callerId,
            ParticipantId = calleeId,
            GAHash = obj.GAHash,
            Protocol = schemaProtocol,
            Video = obj.Video
        };
        await messageSender.PushMessageToPeerAsync(
            new Peer(PeerType.User, calleeId),
            new TUpdateShort { Date = date, Update = new TUpdatePhoneCall { PhoneCall = callRequested } },
            excludeAuthKeyId: null);

        // Return TPhoneCallWaiting to caller
        return new MyTelegram.Schema.Phone.TPhoneCall
        {
            PhoneCall = new TPhoneCallWaiting
            {
                Id = callId,
                AccessHash = accessHash,
                Date = date,
                AdminId = callerId,
                ParticipantId = calleeId,
                Protocol = schemaProtocol,
                Video = obj.Video
            },
            Users = []
        };
    }
}
