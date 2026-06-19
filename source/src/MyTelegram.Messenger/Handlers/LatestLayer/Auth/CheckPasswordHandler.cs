using System.Numerics;
using System.Security.Cryptography;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Auth;
/// <summary>
/// Try logging to an account protected by a <a href="https://corefork.telegram.org/api/srp">2FA password</a>.
/// Possible errors
/// Code Type Description
/// 400 PASSWORD_HASH_INVALID The provided password hash is invalid.
/// 400 SRP_ID_INVALID Invalid SRP ID provided.
/// <para><c>See <a href="https://corefork.telegram.org/method/auth.checkPassword"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✔]
/// </remarks>
internal sealed class CheckPasswordHandler(
    ICacheManager<SrpSessionCacheItem> srpCache,
    IUserAppService userAppService,
    IUserConverterService userConverterService,
    ILayeredService<IAuthorizationConverter> layeredService,
    IEventBus eventBus)
    : RpcResultObjectHandler<MyTelegram.Schema.Auth.RequestCheckPassword, MyTelegram.Schema.Auth.IAuthorization>
{
    protected override async Task<MyTelegram.Schema.Auth.IAuthorization> HandleCoreAsync(
        IRequestInput input,
        MyTelegram.Schema.Auth.RequestCheckPassword obj)
    {
        if (obj.Password is not TInputCheckPasswordSRP srp)
        {
            RpcErrors.RpcErrors400.PasswordHashInvalid.ThrowRpcError();
            return null!;
        }

        var cacheKey = SrpSessionCacheItem.GetCacheKey(srp.SrpId);
        var session = await srpCache.GetAsync(cacheKey);
        if (session == null)
        {
            RpcErrors.RpcErrors400.SrpIdInvalid.ThrowRpcError();
            return null!;
        }

        await srpCache.RemoveAsync(cacheKey);

        var aBytes = srp.A.ToArray();
        var m1Bytes = srp.M1.ToArray();

        if (!VerifySrp(aBytes, m1Bytes, session))
        {
            RpcErrors.RpcErrors400.PasswordHashInvalid.ThrowRpcError();
            return null!;
        }

        var userId = input.UserId;
        var userReadModel = await userAppService.GetAsync(userId, throwIfNotExists: false);
        if (userReadModel == null)
        {
            RpcErrors.RpcErrors400.PasswordHashInvalid.ThrowRpcError();
            return null!;
        }

        await eventBus.PublishAsync(new UserSignInSuccessEvent(
            input.ReqMsgId,
            input.AuthKeyId,
            input.PermAuthKeyId,
            userId,
            PasswordState.Verified));

        ILayeredUser user = userConverterService.ToUser(input, userReadModel, layer: input.Layer);
        user.Self = true;

        return layeredService.GetConverter(input.Layer).CreateAuthorization(user);
    }

    private static bool VerifySrp(byte[] aBytes, byte[] m1Bytes, SrpSessionCacheItem session)
    {
        var p = session.P;
        var g = session.G;
        var v = session.V;
        var smallB = session.SmallB;
        var B = session.B;
        var salt1 = session.Salt1;
        var salt2 = session.Salt2;

        var gBytes = new byte[256];
        var gBig = new BigInteger(g);
        var gBeBytes = gBig.ToByteArray(isUnsigned: true, isBigEndian: true);
        Array.Copy(gBeBytes, 0, gBytes, 256 - gBeBytes.Length, gBeBytes.Length);

        var pBig = new BigInteger(p, isUnsigned: true, isBigEndian: true);
        var vBig = new BigInteger(v, isUnsigned: true, isBigEndian: true);
        var bBig = new BigInteger(smallB, isUnsigned: true, isBigEndian: true);
        var aBig = new BigInteger(aBytes, isUnsigned: true, isBigEndian: true);

        if (aBig <= BigInteger.Zero || aBig >= pBig)
            return false;

        var aPad = PadTo256(aBig);
        var bPad = B;

        var uBytes = SHA256.HashData(aPad.Concat(bPad).ToArray());
        var uBig = new BigInteger(uBytes, isUnsigned: true, isBigEndian: true);

        var vuBig = BigInteger.ModPow(vBig, uBig, pBig);
        var avuBig = BigInteger.Remainder(aBig * vuBig, pBig);
        var sBig = BigInteger.ModPow(avuBig, bBig, pBig);

        var sPad = PadTo256(sBig);
        var K = SHA256.HashData(sPad);

        var hP = SHA256.HashData(p);
        var hG = SHA256.HashData(gBytes);
        var hPxorHG = new byte[32];
        for (var i = 0; i < 32; i++)
            hPxorHG[i] = (byte)(hP[i] ^ hG[i]);

        var hSalt1 = SHA256.HashData(salt1);
        var hSalt2 = SHA256.HashData(salt2);

        var m1Input = hPxorHG
            .Concat(hSalt1)
            .Concat(hSalt2)
            .Concat(aPad)
            .Concat(bPad)
            .Concat(K)
            .ToArray();

        var m1Expected = SHA256.HashData(m1Input);
        return m1Expected.SequenceEqual(m1Bytes);
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
