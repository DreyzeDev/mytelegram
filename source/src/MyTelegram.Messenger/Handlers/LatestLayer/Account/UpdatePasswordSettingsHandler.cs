namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;
/// <summary>
/// Set a new 2FA password
/// <para><c>See <a href="https://corefork.telegram.org/method/account.updatePasswordSettings"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class UpdatePasswordSettingsHandler(ICommandBus commandBus)
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestUpdatePasswordSettings, IBool>
{
    protected override async Task<IBool> HandleCoreAsync(
        IRequestInput input,
        MyTelegram.Schema.Account.RequestUpdatePasswordSettings obj)
    {
        PasswordSetting? passwordSetting = null;

        if (obj.NewSettings is TPasswordInputSettings settings && settings.NewAlgo != null)
        {
            if (settings.NewAlgo is TPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow kdf
                && settings.NewPasswordHash != null)
            {
                var kdfDomain = new PasswordKdfAlgoSha256Sha256Pbkdf2Hmacsha512Iter100000Sha256ModPow(
                    kdf.Salt1,
                    kdf.Salt2,
                    kdf.G,
                    kdf.P.ToArray());

                var secureSettings = new SecureSecretSetting([], [], 0);

                passwordSetting = new PasswordSetting(
                    kdfDomain,
                    settings.NewPasswordHash,
                    settings.Hint ?? string.Empty,
                    settings.Email ?? string.Empty,
                    secureSettings);
            }
        }

        var command = new UpdatePasswordCommand(
            UserId.Create(input.UserId),
            input.ToRequestInfo(),
            input.UserId,
            passwordSetting);

        await commandBus.PublishAsync(command);
        return new TBoolTrue();
    }
}
