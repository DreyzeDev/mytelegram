namespace MyTelegram.QueryHandlers.InMemory.Privacy;

public class GetPrivacyQueryHandler(IQueryOnlyReadModelStore<PrivacyReadModel> store)
    : IQueryHandler<GetPrivacyQuery, IPrivacyReadModel?>
{
    public async Task<IPrivacyReadModel?> ExecuteQueryAsync(GetPrivacyQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.UserId == query.UserId && p.PrivacyType == query.PrivacyType, cancellationToken);
}
