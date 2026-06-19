namespace MyTelegram.QueryHandlers.MongoDB.ChatlistInvite;

public class GetChatlistInviteBySlugQueryHandler(IQueryOnlyReadModelStore<ChatlistInviteReadModel> store)
    : IQueryHandler<GetChatlistInviteBySlugQuery, IChatlistInviteReadModel?>
{
    public async Task<IChatlistInviteReadModel?> ExecuteQueryAsync(GetChatlistInviteBySlugQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.Slug == query.Slug, cancellationToken);
}
