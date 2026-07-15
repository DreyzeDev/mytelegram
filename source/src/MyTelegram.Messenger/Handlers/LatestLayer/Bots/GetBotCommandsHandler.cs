namespace MyTelegram.Messenger.Handlers.LatestLayer.Bots;

/// <summary>
/// Obtain a list of bot commands for the specified bot scope and language code
/// Possible errors
/// Code Type Description
/// 400 USER_BOT_REQUIRED This method can only be called by a bot.
/// <para><c>See <a href="https://corefork.telegram.org/method/bots.getBotCommands"/></c></para>
/// </summary>
/// <remarks>
/// Access: [User ✖] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class GetBotCommandsHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Bots.RequestGetBotCommands, TVector<MyTelegram.Schema.IBotCommand>>
{
    protected override async Task<TVector<MyTelegram.Schema.IBotCommand>> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Bots.RequestGetBotCommands obj)
    {
        var bot = await queryProcessor.ProcessAsync(new GetBotByIdQuery(input.UserId), default);
        if (bot == null)
            RpcErrors.RpcErrors400.BotInvalid.ThrowRpcError();

        var commands = bot.Commands.Select(c => new MyTelegram.Schema.TBotCommand
        {
            Command = c.Command,
            Description = c.Description,
        }).ToList<MyTelegram.Schema.IBotCommand>();

        return new TVector<MyTelegram.Schema.IBotCommand>(commands);
    }
}