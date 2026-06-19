namespace MyTelegram.Domain.Aggregates.QuickReplyShortcut;

[EnableAutoGeneration]
public class QuickReplyShortcutAggregate : AggregateRoot<QuickReplyShortcutAggregate, QuickReplyShortcutId>
{
    private readonly QuickReplyShortcutState _state = new();

    public QuickReplyShortcutAggregate(QuickReplyShortcutId id) : base(id)
    {
        Register(_state);
    }

    public void CreateShortcut(long userId, int shortcutId, string name)
    {
        if (IsNew)
            Emit(new QuickReplyShortcutCreatedEvent(userId, shortcutId, name));
    }

    public void EditShortcut(string name)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new QuickReplyShortcutEditedEvent(name));
    }

    public void DeleteShortcut()
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new QuickReplyShortcutDeletedEvent());
    }
}
