namespace MyTelegram.QueryHandlers.MongoDB.QuickReplyShortcut;

public class GetQuickReplyShortcutsByUserIdQueryHandler(IQueryOnlyReadModelStore<QuickReplyShortcutReadModel> store)
    : IQueryHandler<GetQuickReplyShortcutsByUserIdQuery, IReadOnlyCollection<IQuickReplyReadModel>>
{
    public async Task<IReadOnlyCollection<IQuickReplyReadModel>> ExecuteQueryAsync(GetQuickReplyShortcutsByUserIdQuery query, CancellationToken cancellationToken)
        => await store.FindAsync(p => p.UserId == query.UserId, cancellationToken: cancellationToken);
}
