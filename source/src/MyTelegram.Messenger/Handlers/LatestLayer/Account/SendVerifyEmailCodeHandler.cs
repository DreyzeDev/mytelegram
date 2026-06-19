namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;

internal sealed class SendVerifyEmailCodeHandler(
    IEmailSender emailSender,
    ICacheManager<EmailCodeCacheItem> cacheManager,
    IOptionsMonitor<MyTelegramMessengerServerOptions> options,
    IRandomHelper randomHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestSendVerifyEmailCode, MyTelegram.Schema.Account.ISentEmailCode>
{
    protected override async Task<MyTelegram.Schema.Account.ISentEmailCode> HandleCoreAsync(
        IRequestInput input, MyTelegram.Schema.Account.RequestSendVerifyEmailCode obj)
    {
        var email = obj.Email?.Trim();
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            RpcErrors.RpcErrors400.EmailInvalid.ThrowRpcError();

        var sessionKey = GetSessionKey(obj.Purpose);

        var code = options.CurrentValue.FixedEmailVerificationCode;
        if (string.IsNullOrWhiteSpace(code))
            code = randomHelper.GenerateRandomNumber(6);

        var ttl = options.CurrentValue.VerificationCodeExpirationSeconds;
        await cacheManager.SetAsync(EmailCodeCacheItem.GetCacheKey(sessionKey), new EmailCodeCacheItem(email!, code), ttl);

        await emailSender.SendAsync(email!, "Your verification code", $"Your verification code is: {code}");

        var atIndex = email!.IndexOf('@');
        var maskedLocal = email[..1] + new string('*', Math.Max(0, atIndex - 1));
        var emailPattern = maskedLocal + email[atIndex..];

        return new MyTelegram.Schema.Account.TSentEmailCode
        {
            EmailPattern = emailPattern,
            Length = code.Length
        };
    }

    private static string GetSessionKey(IEmailVerifyPurpose purpose) => purpose switch
    {
        TEmailVerifyPurposeLoginSetup s => s.PhoneCodeHash,
        TEmailVerifyPurposeLoginChange => "login_change",
        _ => "default"
    };
}
