namespace MyTelegram.QueryHandlers.InMemory.Bot;

public class GetBotByTokenQueryHandler(IQueryOnlyReadModelStore<BotReadModel> store)
    : IQueryHandler<GetBotByTokenQuery, IBotReadModel?>
{
    public async Task<IBotReadModel?> ExecuteQueryAsync(GetBotByTokenQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.Token == query.Token, cancellationToken);
}
