namespace MyTelegram.Messenger.Handlers.LatestLayer.Bots;
/// <summary>
/// Get localized name, about text and description of a bot (or of the current account, if called by a bot).
/// Possible errors
/// Code Type Description
/// 400 BOT_INVALID This is not a valid bot.
/// 400 LANG_CODE_INVALID The specified language code is invalid.
/// 400 USER_BOT_INVALID User accounts must provide the <code>bot</code> method parameter when calling this method. If there is no such method parameter, this method can only be invoked by bot accounts.
/// <para><c>See <a href="https://corefork.telegram.org/method/bots.getBotInfo"/></c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class GetBotInfoHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Bots.RequestGetBotInfo, MyTelegram.Schema.Bots.IBotInfo>
{
    protected override async Task<MyTelegram.Schema.Bots.IBotInfo> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Bots.RequestGetBotInfo obj)
    {
        long botUserId;
        if (obj.Bot is TInputUser inputUser)
            botUserId = inputUser.UserId;
        else
            botUserId = input.UserId; // Called by bot itself

        var bot = await queryProcessor.ProcessAsync(new GetBotByIdQuery(botUserId), default);
        if (bot == null)
            RpcErrors.RpcErrors400.BotInvalid.ThrowRpcError();

        return new MyTelegram.Schema.Bots.TBotInfo
        {
            Name = bot!.BotName,
            About = bot.About ?? string.Empty,
            Description = bot.Description ?? string.Empty,
        };
    }
}