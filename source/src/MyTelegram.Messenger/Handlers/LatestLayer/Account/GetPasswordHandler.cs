using System.Numerics;
using System.Security.Cryptography;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;
/// <summary>
/// Obtain configuration for two-factor authorization with password
/// <para><c>See <a href="https://corefork.telegram.org/method/account.getPassword"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✔]
/// </remarks>
internal sealed class GetPasswordHandler(
    IRandomHelper randomHelper,
    IQueryProcessor queryProcessor,
    ICacheManager<SrpSessionCacheItem> srpCache)
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestGetPassword, MyTelegram.Schema.Account.IPassword>
{
    protected override async Task<MyTelegram.Schema.Account.IPassword> HandleCoreAsync(
        IRequestInput input,
        MyTelegram.Schema.Account.RequestGetPassword obj)
    {
        var password = new TPassword();
        var secureRandom = randomHelper.NextBytes(256);
        password.SecureRandom = secureRandom;
        password.NewSecureAlgo = new TSecurePasswordKdfAlgoUnknown();

        var pwdModel = input.UserId > 0
            ? await queryProcessor.ProcessAsync(new GetUserPasswordQuery(input.UserId), default)
            : null;

        if (pwdModel is { HasPassword: true })
        {
            var p = AuthConsts.Dh2048P;
            var g = 3;
            var v = pwdModel.PasswordHash;
            var salt1 = pwdModel.SrpData.Salt1;
            var salt2 = pwdModel.SrpData.Salt2;

            var gBytes = new byte[256];
            var gBig = new BigInteger(g);
            var gBeBytes = gBig.ToByteArray(isUnsigned: true, isBigEndian: true);
            Array.Copy(gBeBytes, 0, gBytes, 256 - gBeBytes.Length, gBeBytes.Length);

            var pBig = new BigInteger(p, isUnsigned: true, isBigEndian: true);
            var vBig = new BigInteger(v, isUnsigned: true, isBigEndian: true);

            byte[] smallB;
            BigInteger gBBig;
            while (true)
            {
                smallB = randomHelper.NextBytes(256);
                var bBig = new BigInteger(smallB, isUnsigned: true, isBigEndian: true);
                gBBig = BigInteger.ModPow(gBig, bBig, pBig);
                if (gBBig > BigInteger.One && gBBig < pBig - BigInteger.One)
                    break;
            }

            var kBytes = SHA256.HashData(p.Concat(gBytes).ToArray());
            var kBig = new BigInteger(kBytes, isUnsigned: true, isBigEndian: true);

            var bSmallBig = new BigInteger(smallB, isUnsigned: true, isBigEndian: true);
            var gBSmall = BigInteger.ModPow(gBig, bSmallBig, pBig);
            var kV = BigInteger.Remainder(kBig * vBig, pBig);
            var BBig = BigInteger.Remainder(kV + gBSmall, pBig);

            var BBytes = PadTo256(BBig);
            var srpId = randomHelper.NextInt64();

            await srpCache.SetAsync(
                SrpSessionCacheItem.GetCacheKey(srpId),
                new SrpSessionCacheItem(srpId, BBytes, smallB, v, salt1, salt2, g, p),
                ttlInSeconds: 300);

            password.HasPassword = true;
            password.Hint = pwdModel.Hint;
            password.SrpId = srpId;
            password.SrpB = BBytes;
            password.CurrentAlgo = new TPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow
            {
                Salt1 = salt1,
                Salt2 = salt2,
                G = g,
                P = p
            };
            password.NewAlgo = new TPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow
            {
                Salt1 = salt1,
                Salt2 = salt2,
                G = g,
                P = p
            };
        }
        else
        {
            password.HasPassword = false;
            password.NewAlgo = new TPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow
            {
                Salt1 = randomHelper.NextBytes(8),
                Salt2 = AuthConsts.SecureAlgoSalt,
                G = 3,
                P = AuthConsts.Dh2048P
            };
        }

        return password;
    }

    private static byte[] PadTo256(BigInteger value)
    {
        var bytes = value.ToByteArray(isUnsigned: true, isBigEndian: true);
        if (bytes.Length == 256)
            return bytes;
        var padded = new byte[256];
        Array.Copy(bytes, 0, padded, 256 - bytes.Length, bytes.Length);
        return padded;
    }
}
