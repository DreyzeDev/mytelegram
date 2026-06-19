namespace MyTelegram.Domain.Aggregates.BusinessChatLink;

public class BusinessChatLinkState : AggregateState<BusinessChatLinkAggregate, BusinessChatLinkId, BusinessChatLinkState>,
    IApply<BusinessChatLinkCreatedEvent>,
    IApply<BusinessChatLinkEditedEvent>,
    IApply<BusinessChatLinkDeletedEvent>
{
    public long UserId { get; private set; }
    public string Slug { get; private set; } = null!;
    public string Message { get; private set; } = string.Empty;
    public string? EntitiesJson { get; private set; }
    public string? Title { get; private set; }
    public int Views { get; private set; }
    public bool Deleted { get; private set; }

    public void Apply(BusinessChatLinkCreatedEvent e)
    {
        UserId = e.UserId;
        Slug = e.Slug;
        Message = e.Message;
        EntitiesJson = e.EntitiesJson;
        Title = e.Title;
    }

    public void Apply(BusinessChatLinkEditedEvent e)
    {
        Message = e.Message;
        EntitiesJson = e.EntitiesJson;
        Title = e.Title;
    }

    public void Apply(BusinessChatLinkDeletedEvent e)
    {
        Deleted = true;
    }
}
