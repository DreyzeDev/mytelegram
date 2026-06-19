namespace MyTelegram.QueryHandlers.InMemory.BusinessChatLink;

public class GetBusinessChatLinkBySlugQueryHandler(IQueryOnlyReadModelStore<BusinessChatLinkReadModel> store)
    : IQueryHandler<GetBusinessChatLinkBySlugQuery, IBusinessChatLinkReadModel?>
{
    public async Task<IBusinessChatLinkReadModel?> ExecuteQueryAsync(GetBusinessChatLinkBySlugQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.Slug == query.Slug, cancellationToken);
}
