namespace MyTelegram.QueryHandlers.InMemory.PhoneCall;

public class GetPhoneCallByIdQueryHandler(IQueryOnlyReadModelStore<PhoneCallReadModel> store)
    : IQueryHandler<GetPhoneCallByIdQuery, IPhoneCallReadModel?>
{
    public async Task<IPhoneCallReadModel?> ExecuteQueryAsync(GetPhoneCallByIdQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.CallId == query.CallId, cancellationToken);
}
