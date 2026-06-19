namespace MyTelegram.ReadModel.Impl;

public class BusinessChatLinkReadModel : ReadModelBase, IBusinessChatLinkReadModel,
    IAmReadModelFor<BusinessChatLinkAggregate, BusinessChatLinkId, BusinessChatLinkCreatedEvent>,
    IAmReadModelFor<BusinessChatLinkAggregate, BusinessChatLinkId, BusinessChatLinkEditedEvent>,
    IAmReadModelFor<BusinessChatLinkAggregate, BusinessChatLinkId, BusinessChatLinkDeletedEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }
    public long UserId { get; private set; }
    public string Slug { get; private set; } = null!;
    public string Message { get; private set; } = string.Empty;
    public string? EntitiesJson { get; private set; }
    public string? Title { get; private set; }
    public int Views { get; private set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<BusinessChatLinkAggregate, BusinessChatLinkId, BusinessChatLinkCreatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Id = domainEvent.AggregateIdentity.Value;
        UserId = e.UserId;
        Slug = e.Slug;
        Message = e.Message;
        EntitiesJson = e.EntitiesJson;
        Title = e.Title;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<BusinessChatLinkAggregate, BusinessChatLinkId, BusinessChatLinkEditedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Message = e.Message;
        EntitiesJson = e.EntitiesJson;
        Title = e.Title;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<BusinessChatLinkAggregate, BusinessChatLinkId, BusinessChatLinkDeletedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        context.MarkForDeletion();
        return Task.CompletedTask;
    }
}
