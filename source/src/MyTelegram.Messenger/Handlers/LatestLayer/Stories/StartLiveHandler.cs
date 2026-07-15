namespace MyTelegram.Messenger.Handlers.Stories;
/// <summary>
/// Start a live story (video stream). Requires Premium.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.startLive"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class StartLiveHandler : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestStartLive, MyTelegram.Schema.IUpdates>, IObjectHandler
{
    protected override Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestStartLive obj)
    {
        RpcErrors.RpcErrors400.PremiumAccountRequired.ThrowRpcError();
        return default!;
    }
}
