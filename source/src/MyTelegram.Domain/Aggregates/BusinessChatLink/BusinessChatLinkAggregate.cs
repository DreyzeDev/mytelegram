namespace MyTelegram.Domain.Aggregates.BusinessChatLink;

[EnableAutoGeneration]
public class BusinessChatLinkAggregate : AggregateRoot<BusinessChatLinkAggregate, BusinessChatLinkId>
{
    private readonly BusinessChatLinkState _state = new();

    public BusinessChatLinkAggregate(BusinessChatLinkId id) : base(id)
    {
        Register(_state);
    }

    public void CreateLink(long userId, string slug, string message,
        string? entitiesJson, string? title)
    {
        if (IsNew)
            Emit(new BusinessChatLinkCreatedEvent(userId, slug, message, entitiesJson, title));
    }

    public void EditLink(string message, string? entitiesJson, string? title)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new BusinessChatLinkEditedEvent(message, entitiesJson, title));
    }

    public void DeleteLink()
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new BusinessChatLinkDeletedEvent());
    }
}
