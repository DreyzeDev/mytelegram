namespace MyTelegram.QueryHandlers.InMemory.Bot;

public class GetMyBotQueryHandler(IQueryOnlyReadModelStore<BotReadModel> store)
    : IQueryHandler<GetMyBotQuery, IBotReadModel?>
{
    public async Task<IBotReadModel?> ExecuteQueryAsync(GetMyBotQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.OwnerUserId == query.OwnerUserId && p.BotUserId == query.BotUserId, cancellationToken);
}
