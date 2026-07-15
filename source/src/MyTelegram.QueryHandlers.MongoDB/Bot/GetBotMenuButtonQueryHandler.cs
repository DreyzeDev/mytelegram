namespace MyTelegram.QueryHandlers.MongoDB.Bot;

public class GetBotMenuButtonQueryHandler(IQueryOnlyReadModelStore<BotMenuReadModel> store)
    : IQueryHandler<GetBotMenuButtonQuery, IBotMenuReadModel?>
{
    public async Task<IBotMenuReadModel?> ExecuteQueryAsync(GetBotMenuButtonQuery query, CancellationToken cancellationToken)
    {
        if (query.UserId != 0)
            return await store.FirstOrDefaultAsync(p => p.BotUserId == query.BotUserId && p.UserId == query.UserId, cancellationToken);
        return await store.FirstOrDefaultAsync(p => p.BotUserId == query.BotUserId, cancellationToken);
    }
}
