namespace MyTelegram.Messenger.Handlers.LatestLayer.Bots;

/// <summary>
/// Sets the menu button action for a given user or for all users
/// Possible errors
/// Code Type Description
/// 400 BUTTON_INVALID The specified button is invalid.
/// 400 BUTTON_TEXT_INVALID The specified button text is invalid.
/// 400 BUTTON_URL_INVALID Button URL invalid.
/// 400 USER_BOT_REQUIRED This method can only be called by a bot.
/// <para><c>See <a href="https://corefork.telegram.org/method/bots.setBotMenuButton"/></c></para>
/// </summary>
/// <remarks>
/// Access: [User ✖] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class SetBotMenuButtonHandler(ICommandBus commandBus)
    : RpcResultObjectHandler<MyTelegram.Schema.Bots.RequestSetBotMenuButton, IBool>
{
    protected override async Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Bots.RequestSetBotMenuButton obj)
    {
        long targetUserId = 0;
        if (obj.UserId is TInputUser inputUser)
        {
            targetUserId = inputUser.UserId;
        }

        string type = "default";
        string? text = null;
        string? url = null;

        if (obj.Button is TBotMenuButton b)
        {
            type = "web_app";
            text = b.Text;
            url = b.Url;
        }
        else if (obj.Button is TBotMenuButtonCommands)
        {
            type = "commands";
        }

        var command = new SetBotMenuButtonCommand(
            BotId.Create(input.UserId),
            targetUserId,
            input.UserId,
            type, text, url);

        await commandBus.PublishAsync(command, default);

        return new TBoolTrue();
    }
}