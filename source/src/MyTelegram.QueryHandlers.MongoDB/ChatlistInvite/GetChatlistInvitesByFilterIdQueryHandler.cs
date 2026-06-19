namespace MyTelegram.QueryHandlers.MongoDB.ChatlistInvite;

public class GetChatlistInvitesByFilterIdQueryHandler(IQueryOnlyReadModelStore<ChatlistInviteReadModel> store)
    : IQueryHandler<GetChatlistInvitesByFilterIdQuery, IReadOnlyCollection<IChatlistInviteReadModel>>
{
    public async Task<IReadOnlyCollection<IChatlistInviteReadModel>> ExecuteQueryAsync(GetChatlistInvitesByFilterIdQuery query, CancellationToken cancellationToken)
        => await store.FindAsync(p => p.UserId == query.UserId && p.FilterId == query.FilterId, cancellationToken: cancellationToken);
}
