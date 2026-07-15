namespace MyTelegram.Messenger.Handlers.LatestLayer.Phone;
/// <summary>
/// Get phone call configuration (STUN/TURN servers) for libtgvoip.
/// <para><c>See <a href="https://corefork.telegram.org/method/phone.getCallConfig"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetCallConfigHandler : RpcResultObjectHandler<MyTelegram.Schema.Phone.RequestGetCallConfig, MyTelegram.Schema.IDataJSON>
{
    // Minimal config accepted by libtgvoip; clients fall back to P2P when STUN is absent.
    private const string CallConfigJson = """
        {
          "ios": { "hardware_encoder_disabled": true },
          "android": { "hardware_encoder_disabled": true },
          "rtc_endpoints": []
        }
        """;

    protected override Task<MyTelegram.Schema.IDataJSON> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Phone.RequestGetCallConfig obj)
        => Task.FromResult<MyTelegram.Schema.IDataJSON>(new TDataJSON { Data = CallConfigJson });
}
