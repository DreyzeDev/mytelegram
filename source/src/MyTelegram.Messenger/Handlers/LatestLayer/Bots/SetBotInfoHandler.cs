namespace MyTelegram.Messenger.Handlers.LatestLayer.Bots;
/// <summary>
/// Set localized name, about text and description of a bot (or of the current account, if called by a bot).
/// Possible errors
/// Code Type Description
/// 400 BOT_INVALID This is not a valid bot.
/// 400 USER_BOT_INVALID User accounts must provide the <code>bot</code> method parameter when calling this method. If there is no such method parameter, this method can only be invoked by bot accounts.
/// <para><c>See <a href="https://corefork.telegram.org/method/bots.setBotInfo"/></c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class SetBotInfoHandler(ICommandBus commandBus, IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Bots.RequestSetBotInfo, IBool>
{
    protected override async Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Bots.RequestSetBotInfo obj)
    {
        long botUserId;
        if (obj.Bot is TInputUser inputUser)
            botUserId = inputUser.UserId;
        else
            botUserId = input.UserId;

        var bot = await queryProcessor.ProcessAsync(new GetBotByIdQuery(botUserId), default);
        if (bot == null)
            RpcErrors.RpcErrors400.BotInvalid.ThrowRpcError();

        var command = new UpdateBotInfoCommand(
            BotId.Create(botUserId),
            botName: obj.Name,
            about: obj.About,
            description: obj.Description);
        await commandBus.PublishAsync(command, default);

        return new TBoolTrue();
    }
}