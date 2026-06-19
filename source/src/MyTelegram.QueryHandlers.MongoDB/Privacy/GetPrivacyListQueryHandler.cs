namespace MyTelegram.QueryHandlers.MongoDB.Privacy;

public class GetPrivacyListQueryHandler(IQueryOnlyReadModelStore<PrivacyReadModel> store)
    : IQueryHandler<GetPrivacyListQuery, IReadOnlyCollection<IPrivacyReadModel>>
{
    public Task<IReadOnlyCollection<IPrivacyReadModel>> ExecuteQueryAsync(GetPrivacyListQuery query, CancellationToken cancellationToken)
        => store.FindAsync(
            p => query.UserIdList.Contains(p.UserId) && query.PrivacyTypes.Contains(p.PrivacyType),
            p => (IPrivacyReadModel)p,
            cancellationToken: cancellationToken);
}
