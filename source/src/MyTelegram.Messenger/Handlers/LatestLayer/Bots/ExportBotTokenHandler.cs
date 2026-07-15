namespace MyTelegram.Messenger.Handlers.LatestLayer.Bots;

/// <summary>
/// <para><c>See <a href="https://corefork.telegram.org/method/bots.exportBotToken"/></c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class ExportBotTokenHandler(ICommandBus commandBus, IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Bots.RequestExportBotToken, MyTelegram.Schema.Bots.IExportedBotToken>
{
    protected override async Task<MyTelegram.Schema.Bots.IExportedBotToken> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Bots.RequestExportBotToken obj)
    {
        long botUserId;
        if (obj.Bot is TInputUser inputUser)
            botUserId = inputUser.UserId;
        else
            botUserId = input.UserId;

        var bot = await queryProcessor.ProcessAsync(new GetMyBotQuery(input.UserId, botUserId), default);
        if (bot == null)
            RpcErrors.RpcErrors400.BotInvalid.ThrowRpcError();

        var token = bot.Token;

        if (obj.Revoke)
        {
            token = $"{botUserId}:{Guid.NewGuid():N}";
            var command = new UpdateBotTokenCommand(BotId.Create(botUserId), token);
            await commandBus.PublishAsync(command, default);
        }

        return new MyTelegram.Schema.Bots.TExportedBotToken
        {
            Token = token,
        };
    }
}
