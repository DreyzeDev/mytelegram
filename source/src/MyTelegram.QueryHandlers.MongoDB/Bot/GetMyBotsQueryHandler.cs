namespace MyTelegram.QueryHandlers.MongoDB.Bot;

public class GetMyBotsQueryHandler(IQueryOnlyReadModelStore<BotReadModel> store)
    : IQueryHandler<GetMyBotsQuery, IReadOnlyCollection<IBotReadModel>>
{
    public async Task<IReadOnlyCollection<IBotReadModel>> ExecuteQueryAsync(GetMyBotsQuery query, CancellationToken cancellationToken)
    {
        var results = await store.FindAsync(p => p.OwnerUserId == query.OwnerUserId, cancellationToken: cancellationToken);
        return results.Cast<IBotReadModel>().ToList();
    }
}
