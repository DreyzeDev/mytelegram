namespace MyTelegram.ReadModel.Impl;

public class BotMenuReadModel : ReadModelBase, IBotMenuReadModel,
    IAmReadModelFor<BotAggregate, BotId, BotMenuButtonSetEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }
    public long BotUserId { get; private set; }
    public long UserId { get; private set; }
    public IBotMenuButton Menu { get; private set; } = new TBotMenuButtonDefault();

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<BotAggregate, BotId, BotMenuButtonSetEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Id = $"botmenu-{e.BotUserId}-{e.UserId}";
        BotUserId = e.BotUserId;
        UserId = e.UserId;
        Menu = e.MenuButtonType switch
        {
            "commands" => new MyTelegram.Schema.TBotMenuButtonCommands(),
            "web_app" => new MyTelegram.Schema.TBotMenuButton { Text = e.Text ?? "", Url = e.Url ?? "" },
            _ => new MyTelegram.Schema.TBotMenuButtonDefault()
        };
        return Task.CompletedTask;
    }
}
