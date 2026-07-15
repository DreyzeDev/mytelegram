namespace MyTelegram.Messenger.Handlers.LatestLayer.Bots;

/// <summary>
/// Set bot command list
/// Possible errors
/// Code Type Description
/// 400 BOT_COMMAND_DESCRIPTION_INVALID The specified command description is invalid.
/// 400 BOT_COMMAND_INVALID The specified command is invalid.
/// 400 LANG_CODE_INVALID The specified language code is invalid.
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// 400 USER_BOT_REQUIRED This method can only be called by a bot.
/// 400 USER_ID_INVALID The provided user ID is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/bots.setBotCommands"/></c></para>
/// </summary>
/// <remarks>
/// Access: [User ✖] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class SetBotCommandsHandler(ICommandBus commandBus)
    : RpcResultObjectHandler<MyTelegram.Schema.Bots.RequestSetBotCommands, IBool>
{
    protected override async Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Bots.RequestSetBotCommands obj)
    {
        var domainCommands = obj.Commands.Select(c => new MyTelegram.BotCommand(c.Command, c.Description)).ToList();

        var command = new UpdateBotCommandsCommand(BotId.Create(input.UserId), domainCommands);
        await commandBus.PublishAsync(command, default);

        return new TBoolTrue();
    }
}