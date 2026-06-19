namespace MyTelegram.Domain.Aggregates.ChatlistInvite;

public class ChatlistInviteState : AggregateState<ChatlistInviteAggregate, ChatlistInviteId, ChatlistInviteState>,
    IApply<ChatlistInviteCreatedEvent>,
    IApply<ChatlistInviteEditedEvent>,
    IApply<ChatlistInviteDeletedEvent>
{
    public long UserId { get; private set; }
    public int FilterId { get; private set; }
    public string Slug { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string PeersJson { get; private set; } = "[]";
    public string? Emoticon { get; private set; }

    public void Apply(ChatlistInviteCreatedEvent e)
    {
        UserId = e.UserId;
        FilterId = e.FilterId;
        Slug = e.Slug;
        Title = e.Title;
        PeersJson = e.PeersJson;
        Emoticon = e.Emoticon;
    }

    public void Apply(ChatlistInviteEditedEvent e)
    {
        Title = e.Title;
        if (e.PeersJson != null)
            PeersJson = e.PeersJson;
    }

    public void Apply(ChatlistInviteDeletedEvent e) { }
}
