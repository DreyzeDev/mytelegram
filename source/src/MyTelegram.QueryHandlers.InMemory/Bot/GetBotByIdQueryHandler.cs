namespace MyTelegram.QueryHandlers.InMemory.Bot;

public class GetBotByIdQueryHandler(IQueryOnlyReadModelStore<BotReadModel> store)
    : IQueryHandler<GetBotByIdQuery, IBotReadModel?>
{
    public async Task<IBotReadModel?> ExecuteQueryAsync(GetBotByIdQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.BotUserId == query.BotUserId, cancellationToken);
}
