namespace MyTelegram.QueryHandlers.InMemory.BusinessChatLink;

public class GetBusinessChatLinksByUserIdQueryHandler(IQueryOnlyReadModelStore<BusinessChatLinkReadModel> store)
    : IQueryHandler<GetBusinessChatLinksByUserIdQuery, IReadOnlyCollection<IBusinessChatLinkReadModel>>
{
    public async Task<IReadOnlyCollection<IBusinessChatLinkReadModel>> ExecuteQueryAsync(GetBusinessChatLinksByUserIdQuery query, CancellationToken cancellationToken)
        => await store.FindAsync(p => p.UserId == query.UserId, cancellationToken: cancellationToken);
}
