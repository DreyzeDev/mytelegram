namespace MyTelegram.Domain.Aggregates.Bot;

public class BotState : AggregateState<BotAggregate, BotId, BotState>,
    IApply<BotCreatedEvent>,
    IApply<BotCommandsUpdatedEvent>,
    IApply<BotInfoUpdatedEvent>,
    IApply<BotMenuButtonSetEvent>,
    IApply<BotTokenUpdatedEvent>
{
    public long BotUserId { get; private set; }
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

    public void Apply(BotCreatedEvent e)
    {
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
    }

    public void Apply(BotCommandsUpdatedEvent e)
    {
        Commands = e.Commands;
    }

    public void Apply(BotInfoUpdatedEvent e)
    {
        if (e.BotName != null) BotName = e.BotName;
        if (e.About != null) About = e.About;
        if (e.Description != null) Description = e.Description;
    }

    public void Apply(BotMenuButtonSetEvent e)
    {
        // Menu button info stored in BotMenuReadModel (keyed by botUserId+userId)
    }

    public void Apply(BotTokenUpdatedEvent e)
    {
        Token = e.Token;
    }
}
