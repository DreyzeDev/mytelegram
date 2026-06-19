namespace MyTelegram.Core;

public record SrpSessionCacheItem(
    long SrpId,
    byte[] B,
    byte[] SmallB,
    byte[] V,
    byte[] Salt1,
    byte[] Salt2,
    int G,
    byte[] P)
{
    public static string GetCacheKey(long srpId) =>
        MyCacheKey.With("srp2fa", srpId.ToString());
}
