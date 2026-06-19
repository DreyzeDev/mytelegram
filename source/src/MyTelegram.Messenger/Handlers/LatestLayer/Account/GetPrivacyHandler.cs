namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;
/// <summary>
/// Get privacy settings of current account
/// Possible errors
/// Code Type Description
/// 400 PRIVACY_KEY_INVALID The privacy key is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/account.getPrivacy"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetPrivacyHandler(IPrivacyAppService privacyAppService)
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestGetPrivacy, MyTelegram.Schema.Account.IPrivacyRules>
{
    protected override async Task<MyTelegram.Schema.Account.IPrivacyRules> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Account.RequestGetPrivacy obj)
    {
        var rules = await privacyAppService.GetPrivacyRulesAsync(input.UserId, obj.Key);
        return new TPrivacyRules
        {
            Rules = new TVector<IPrivacyRule>(rules),
            Users = new TVector<IUser>(),
            Chats = new TVector<IChat>()
        };
    }
}
