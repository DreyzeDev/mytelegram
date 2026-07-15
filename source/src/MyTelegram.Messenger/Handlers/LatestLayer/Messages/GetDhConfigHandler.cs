using System.Security.Cryptography;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Returns configuration parameters for Diffie-Hellman key generation. Can also return a random sequence of bytes of required length.
/// Possible errors
/// Code Type Description
/// 400 RANDOM_LENGTH_INVALID Random length invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.getDhConfig"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetDhConfigHandler : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestGetDhConfig, MyTelegram.Schema.Messages.IDhConfig>
{
    // Standard 2048-bit MTProto DH prime (g = 3), the well-known public value used by every
    // MTProto-compatible implementation (see https://corefork.telegram.org/mtproto/security_guidelines and tdlib's default dh_config).
    // This is public configuration data, not a secret.
    private const string DhPrimeHex =
        "C71CAEB9C6B1C9048E6C522F70F13F73980D40238E3E21C14934D037563D930F48198A0AA7C14058229493D22530F4DBFA336F6E0AC925139543AED44CCE7C3720FD51F69458705AC68CD4FE6B6B13ABDC9746512969328454F18FAF8C595F642477FE96BB2A941D5BCD1D4AC8CC49880708FA9B378E3C4F3A9060BEE67CF9A4A4A695811051907E162753B56B0F6B410DBA74D8A84B2A14B3144E0EF1284754FD17ED950D5965B4B9DD46582DB1178D169C6BC465B0D6FF9CA3928FEF5B9AE4E418FC15E83EBEA0F87FA9FF5EED70050DED2849F47BF959D956850CE929851F0D8115F635B105EE2E4E15D04B2454BF6F4FADF034B10403119CD8E3B92FCC5B";

    private const int DhConfigVersion = 1;

    protected override Task<MyTelegram.Schema.Messages.IDhConfig> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestGetDhConfig obj)
    {
        var randomLength = obj.RandomLength;
        if (randomLength <= 0)
        {
            RpcErrors.RpcErrors400.RandomLengthInvalid.ThrowRpcError();
        }

        var randomBytes = new byte[randomLength];
        RandomNumberGenerator.Fill(randomBytes);

        // The DH parameters (p, g) never change on this server, so there's no "not modified" fast path to take -
        // always return the full config together with the requested random bytes.
        MyTelegram.Schema.Messages.IDhConfig result = new TDhConfig
        {
            G = 3,
            P = Convert.FromHexString(DhPrimeHex),
            Version = DhConfigVersion,
            Random = randomBytes
        };

        return Task.FromResult(result);
    }
}