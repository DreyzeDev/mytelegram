namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Activates stories stealth mode (requires Premium).
/// Possible errors
/// Code Type Description
/// 400 PREMIUM_ACCOUNT_REQUIRED A premium account is required to execute this action.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.activateStealthMode"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class ActivateStealthModeHandler : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestActivateStealthMode, MyTelegram.Schema.IUpdates>
{
    protected override Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestActivateStealthMode obj)
    {
        RpcErrors.RpcErrors400.PremiumAccountRequired.ThrowRpcError();
        return default!;
    }
}
