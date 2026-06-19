namespace MyTelegram.Domain.Aggregates.QuickReplyShortcut;

public class QuickReplyShortcutId(string value) : Identity<QuickReplyShortcutId>(value)
{
    public static QuickReplyShortcutId Create(long userId, int shortcutId) =>
        NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"qrs-{userId}-{shortcutId}");
}
