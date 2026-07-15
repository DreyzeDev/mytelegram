namespace MyTelegram.ReadModel.Impl;

public class BotReadModel : ReadModelBase, IBotReadModel,
    IAmReadModelFor<BotAggregate, BotId, BotCreatedEvent>,
    IAmReadModelFor<BotAggregate, BotId, BotCommandsUpdatedEvent>,
    IAmReadModelFor<BotAggregate, BotId, BotInfoUpdatedEvent>,
    IAmReadModelFor<BotAggregate, BotId, BotTokenUpdatedEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }
    public long BotUserId { get; private set; }
    public long UserId => BotUserId;
    public long OwnerUserId { get; private set; }
    public string Token { get; private set; } = null!;
    public string BotName { get; private set; } = null!;
    public string UserName { get; private set; } = null!;
    public string? About { get; private set; }
    public string? Description { get; private set; }
    public bool AllowJoinGroups { get; private set; }
    public bool AllowAccessGroupMessages { get; private set; }
    public bool InlineModeEnabled { get; private set; }
    public string? InlinePlaceholder { get; private set; }
    public List<BotCommand> Commands { get; private set; } = [];
    public string? WebHookUrl { get; private set; }
    public long? DescriptionDocumentId { get; private set; }
    public long? DescriptionPhotoId { get; private set; }
    public bool BusinessModeEnabled { get; private set; }
    public string? PrivacyPolicyUrl { get; private set; }
    public string? MiniAppUrl { get; private set; }
    public int ChatAdminRights { get; private set; }
    public int ChannelAdminRights { get; private set; }
    public long? ProfilePhotoId { get; private set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<BotAggregate, BotId, BotCreatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Id = domainEvent.AggregateIdentity.Value;
        BotUserId = e.BotUserId;
        OwnerUserId = e.OwnerUserId;
        Token = e.Token;
        BotName = e.BotName;
        UserName = e.UserName;
        About = e.About;
        AllowJoinGroups = e.AllowJoinGroups;
        AllowAccessGroupMessages = e.AllowAccessGroupMessages;
        InlineModeEnabled = e.InlineModeEnabled;
        InlinePlaceholder = e.InlinePlaceholder;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<BotAggregate, BotId, BotCommandsUpdatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Commands = domainEvent.AggregateEvent.Commands;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<BotAggregate, BotId, BotInfoUpdatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        if (e.BotName != null) BotName = e.BotName;
        if (e.About != null) About = e.About;
        if (e.Description != null) Description = e.Description;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<BotAggregate, BotId, BotTokenUpdatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Token = domainEvent.AggregateEvent.Token;
        return Task.CompletedTask;
    }
}
