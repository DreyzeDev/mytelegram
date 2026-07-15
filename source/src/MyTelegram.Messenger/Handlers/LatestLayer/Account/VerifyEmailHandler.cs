namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;

internal sealed class VerifyEmailHandler(
    ICacheManager<EmailCodeCacheItem> cacheManager)
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestVerifyEmail, MyTelegram.Schema.Account.IEmailVerified>
{
    protected override async Task<MyTelegram.Schema.Account.IEmailVerified> HandleCoreAsync(
        IRequestInput input, MyTelegram.Schema.Account.RequestVerifyEmail obj)
    {
        var codeObj = obj.Verification as TEmailVerificationCode;
        if (codeObj is null)
            RpcErrors.RpcErrors400.CodeInvalid.ThrowRpcError();

        var sessionKey = GetSessionKey(obj.Purpose);
        var cachedItem = await cacheManager.GetAsync(EmailCodeCacheItem.GetCacheKey(sessionKey));
        if (cachedItem == null)
            RpcErrors.RpcErrors400.EmailVerifyExpired.ThrowRpcError();

        if (!string.Equals(cachedItem!.Code, codeObj!.Code, StringComparison.OrdinalIgnoreCase))
            RpcErrors.RpcErrors400.CodeInvalid.ThrowRpcError();

        await cacheManager.RemoveAsync(EmailCodeCacheItem.GetCacheKey(sessionKey));

        return new MyTelegram.Schema.Account.TEmailVerified { Email = cachedItem.Email };
    }

    private static string GetSessionKey(IEmailVerifyPurpose purpose) => purpose switch
    {
        TEmailVerifyPurposeLoginSetup s => s.PhoneCodeHash,
        TEmailVerifyPurposeLoginChange => "login_change",
        _ => "default"
    };
}
