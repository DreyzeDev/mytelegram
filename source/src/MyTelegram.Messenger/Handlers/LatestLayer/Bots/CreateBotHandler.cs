namespace MyTelegram.Messenger.Handlers.Bots;

/// <summary>
/// Creates a bot
/// <para><c>See <a href="https://corefork.telegram.org/method/bots.createBot"/></c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class CreateBotHandler(ICommandBus commandBus, IQueryProcessor queryProcessor, IRandomHelper randomHelper, IIdGenerator idGenerator)
    : RpcResultObjectHandler<MyTelegram.Schema.Bots.RequestCreateBot, MyTelegram.Schema.IUser>, IObjectHandler
{
    protected override async Task<MyTelegram.Schema.IUser> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Bots.RequestCreateBot obj)
    {
        var ownerUserId = input.UserId;

        // Check username availability
        var existingName = await queryProcessor.ProcessAsync(new GetUserNameByNameQuery(obj.Username), default);
        if (existingName != null)
            RpcErrors.RpcErrors400.UsernameOccupied.ThrowRpcError();

        var maxBotUserId = await queryProcessor.ProcessAsync(new GetMaxBotUserIdQuery(), default);
        var maxUserId = await queryProcessor.ProcessAsync(new GetMaxUserIdQuery(), default);
        var botUserId = Math.Max(maxBotUserId, maxUserId) + 1;

        var accessHash = randomHelper.NextInt64();
        var token = $"{botUserId}:{Guid.NewGuid():N}";

        // Create user account for the bot (bot=true)
        var createUserCommand = new CreateUserCommand(
            UserId.Create(botUserId),
            input.ToRequestInfo() with { UserId = botUserId },
            botUserId,
            accessHash,
            phoneNumber: $"bot_{botUserId}",
            firstName: obj.Name,
            lastName: null,
            userName: obj.Username,
            bot: true
        );
        await commandBus.PublishAsync(createUserCommand, default);

        // Create bot record storing token, commands, webhook, etc.
        var createBotCommand = new CreateBotCommand(
            BotId.Create(botUserId),
            botUserId, ownerUserId, token,
            obj.Name, obj.Username,
            about: null,
            allowJoinGroups: true,
            allowAccessGroupMessages: false,
            inlineModeEnabled: false,
            inlinePlaceholder: null);
        await commandBus.PublishAsync(createBotCommand, default);

        return new TUser
        {
            Id = botUserId,
            AccessHash = accessHash,
            FirstName = obj.Name,
            Username = obj.Username,
            Bot = true,
        };
    }
}
