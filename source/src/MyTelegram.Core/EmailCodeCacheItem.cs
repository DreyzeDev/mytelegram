namespace MyTelegram.Core;

public record EmailCodeCacheItem(string Email, string Code)
{
    // Keyed by phoneCodeHash from TEmailVerifyPurposeLoginSetup so verifyEmail can look it up
    public static string GetCacheKey(string sessionKey) =>
        MyCacheKey.With("email_verify_code", sessionKey.ToLowerInvariant());
}
