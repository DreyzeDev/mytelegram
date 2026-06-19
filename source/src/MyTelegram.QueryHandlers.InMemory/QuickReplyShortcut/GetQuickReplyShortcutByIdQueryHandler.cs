namespace MyTelegram.QueryHandlers.InMemory.QuickReplyShortcut;

public class GetQuickReplyShortcutByIdQueryHandler(IQueryOnlyReadModelStore<QuickReplyShortcutReadModel> store)
    : IQueryHandler<GetQuickReplyShortcutByIdQuery, IQuickReplyReadModel?>
{
    public async Task<IQuickReplyReadModel?> ExecuteQueryAsync(GetQuickReplyShortcutByIdQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.UserId == query.UserId && p.ShortcutId == query.ShortcutId, cancellationToken);
}
