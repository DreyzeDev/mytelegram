namespace MyTelegram.Domain.Aggregates.Bot;

[EnableAutoGeneration]
public class BotAggregate : AggregateRoot<BotAggregate, BotId>
{
    private readonly BotState _state = new();

    public BotAggregate(BotId id) : base(id)
    {
        Register(_state);
    }

    public void CreateBot(long botUserId, long ownerUserId, string token, string botName,
        string userName, string? about, bool allowJoinGroups, bool allowAccessGroupMessages,
        bool inlineModeEnabled, string? inlinePlaceholder)
    {
        if (IsNew)
            Emit(new BotCreatedEvent(botUserId, ownerUserId, token, botName, userName, about,
                allowJoinGroups, allowAccessGroupMessages, inlineModeEnabled, inlinePlaceholder));
    }

    public void UpdateBotCommands(List<BotCommand> commands)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new BotCommandsUpdatedEvent(commands));
    }

    public void UpdateBotInfo(string? botName, string? about, string? description)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new BotInfoUpdatedEvent(botName, about, description));
    }

    public void SetBotMenuButton(long userId, long botUserId, string menuButtonType, string? text, string? url)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new BotMenuButtonSetEvent(userId, botUserId, menuButtonType, text, url));
    }

    public void UpdateBotToken(string token)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new BotTokenUpdatedEvent(token));
    }
}
