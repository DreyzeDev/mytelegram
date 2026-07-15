namespace MyTelegram.QueryHandlers.MongoDB.Bot;

public class GetMaxBotUserIdQueryHandler(IQueryOnlyReadModelStore<BotReadModel> store)
    : IQueryHandler<GetMaxBotUserIdQuery, long>
{
    public async Task<long> ExecuteQueryAsync(GetMaxBotUserIdQuery query, CancellationToken cancellationToken)
    {
        var all = await store.FindAsync(p => true, cancellationToken: cancellationToken);
        return all.Any() ? all.Max(p => p.BotUserId) : 0;
    }
}
