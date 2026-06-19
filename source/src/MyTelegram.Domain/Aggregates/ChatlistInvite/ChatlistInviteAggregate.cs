namespace MyTelegram.Domain.Aggregates.ChatlistInvite;

[EnableAutoGeneration]
public class ChatlistInviteAggregate : AggregateRoot<ChatlistInviteAggregate, ChatlistInviteId>
{
    private readonly ChatlistInviteState _state = new();

    public ChatlistInviteAggregate(ChatlistInviteId id) : base(id)
    {
        Register(_state);
    }

    public void CreateInvite(long userId, int filterId, string slug, string title, string peersJson, string? emoticon)
    {
        if (IsNew)
            Emit(new ChatlistInviteCreatedEvent(userId, filterId, slug, title, peersJson, emoticon));
    }

    public void EditInvite(string title, string? peersJson)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new ChatlistInviteEditedEvent(title, peersJson));
    }

    public void DeleteInvite()
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new ChatlistInviteDeletedEvent());
    }
}
