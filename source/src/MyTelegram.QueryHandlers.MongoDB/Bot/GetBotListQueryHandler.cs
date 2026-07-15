namespace MyTelegram.QueryHandlers.MongoDB.Bot;

public class GetBotListQueryHandler(IQueryOnlyReadModelStore<BotReadModel> store)
    : IQueryHandler<GetBotListQuery, IReadOnlyCollection<IBotReadModel>>
{
    public async Task<IReadOnlyCollection<IBotReadModel>> ExecuteQueryAsync(GetBotListQuery query, CancellationToken cancellationToken)
    {
        var results = await store.FindAsync(p => query.BotUserIds.Contains(p.BotUserId), cancellationToken: cancellationToken);
        return results.Cast<IBotReadModel>().ToList();
    }
}
