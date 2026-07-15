namespace MyTelegram.Messenger.Handlers.LatestLayer.Bots;

/// <summary>
/// Clear bot commands for the specified bot scope and language code
/// Possible errors
/// Code Type Description
/// 400 USER_BOT_REQUIRED This method can only be called by a bot.
/// <para><c>See <a href="https://corefork.telegram.org/method/bots.resetBotCommands"/></c></para>
/// </summary>
/// <remarks>
/// Access: [User ✖] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class ResetBotCommandsHandler(ICommandBus commandBus)
    : RpcResultObjectHandler<MyTelegram.Schema.Bots.RequestResetBotCommands, IBool>
{
    protected override async Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Bots.RequestResetBotCommands obj)
    {
        var command = new UpdateBotCommandsCommand(BotId.Create(input.UserId), new List<MyTelegram.BotCommand>());
        await commandBus.PublishAsync(command, default);

        return new TBoolTrue();
    }
}