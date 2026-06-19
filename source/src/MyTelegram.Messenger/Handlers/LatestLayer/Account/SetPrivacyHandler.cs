namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;
/// <summary>
/// Change privacy settings of current account
/// Possible errors
/// Code Type Description
/// 400 PRIVACY_KEY_INVALID The privacy key is invalid.
/// 400 PRIVACY_TOO_LONG Too many privacy rules were specified, the current limit is 1000.
/// 400 PRIVACY_VALUE_INVALID The specified privacy rule combination is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/account.setPrivacy"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class SetPrivacyHandler(IPrivacyAppService privacyAppService)
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestSetPrivacy, MyTelegram.Schema.Account.IPrivacyRules>
{
    protected override async Task<MyTelegram.Schema.Account.IPrivacyRules> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Account.RequestSetPrivacy obj)
    {
        var output = await privacyAppService.SetPrivacyAsync(input.ToRequestInfo(), input.UserId, obj.Key, obj.Rules);
        return new TPrivacyRules
        {
            Rules = new TVector<IPrivacyRule>(output.Rules),
            Users = new TVector<IUser>(),
            Chats = new TVector<IChat>()
        };
    }
}
