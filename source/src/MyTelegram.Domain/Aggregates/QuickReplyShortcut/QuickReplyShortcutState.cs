namespace MyTelegram.Domain.Aggregates.QuickReplyShortcut;

public class QuickReplyShortcutState : AggregateState<QuickReplyShortcutAggregate, QuickReplyShortcutId, QuickReplyShortcutState>,
    IApply<QuickReplyShortcutCreatedEvent>,
    IApply<QuickReplyShortcutEditedEvent>,
    IApply<QuickReplyShortcutDeletedEvent>
{
    public long UserId { get; private set; }
    public int ShortcutId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public void Apply(QuickReplyShortcutCreatedEvent e)
    {
        UserId = e.UserId;
        ShortcutId = e.ShortcutId;
        Name = e.Name;
    }

    public void Apply(QuickReplyShortcutEditedEvent e)
    {
        Name = e.Name;
    }

    public void Apply(QuickReplyShortcutDeletedEvent e) { }
}
