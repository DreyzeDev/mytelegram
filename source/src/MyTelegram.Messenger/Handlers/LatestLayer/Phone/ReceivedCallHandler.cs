namespace MyTelegram.Messenger.Handlers.LatestLayer.Phone;
/// <summary>
/// Notify the server that the user received an incoming call (used to mark busy on other devices).
/// Possible errors
/// Code Type Description
/// 400 CALL_PEER_INVALID The provided call peer object is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/phone.receivedCall"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class ReceivedCallHandler : RpcResultObjectHandler<MyTelegram.Schema.Phone.RequestReceivedCall, IBool>
{
    protected override Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Phone.RequestReceivedCall obj)
        => Task.FromResult<IBool>(new TBoolTrue());
}
