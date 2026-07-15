namespace MyTelegram.Messenger.Handlers.LatestLayer.Bots;

/// <summary>
/// Gets the menu button action for a given user or for all users
/// Possible errors
/// Code Type Description
/// 400 USER_BOT_REQUIRED This method can only be called by a bot.
/// <para><c>See <a href="https://corefork.telegram.org/method/bots.getBotMenuButton"/></c></para>
/// </summary>
/// <remarks>
/// Access: [User ✖] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class GetBotMenuButtonHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Bots.RequestGetBotMenuButton, MyTelegram.Schema.IBotMenuButton>
{
    protected override async Task<MyTelegram.Schema.IBotMenuButton> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Bots.RequestGetBotMenuButton obj)
    {
        long targetUserId = 0;
        if (obj.UserId is TInputUser inputUser)
        {
            targetUserId = inputUser.UserId;
        }

        var menu = await queryProcessor.ProcessAsync(new GetBotMenuButtonQuery(input.UserId, targetUserId), default);
        
        if (menu != null && menu.Menu != null)
        {
            return menu.Menu;
        }

        return new MyTelegram.Schema.TBotMenuButtonDefault();
    }
}